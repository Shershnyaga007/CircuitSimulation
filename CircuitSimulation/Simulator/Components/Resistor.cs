namespace CircuitSimulation.Simulator.Components;

/// <summary>
/// Класс резистора.
/// Резистор описывается законом Ома: V = I * R.
/// При штамповке в уравнениях метода контурного анализа добавляется вклад сопротивления.
/// Если резистор соединяет два свободных контура, то он вносит вклад R к диагональным элементам
/// и -R к перекрёстным (сцепляющим) элементам матрицы A.
/// Если один из контуров задан (форсирован), его вклад переносится в вектор B.
/// </summary>
public class Resistor : CircuitComponent
{
    public double Resistance;

    public Resistor(int mesh1, int mesh2, double resistance) : base(mesh1, mesh2)
    {
        Resistance = resistance;
    }

    public override void Stamp(double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state, double dt)
    {
        double coeff = Resistance;
        StampContribution(Mesh1, Mesh2, coeff, A, B, freeMeshMap, state);
        StampContribution(Mesh2, Mesh1, coeff, A, B, freeMeshMap, state);
    }

    /// <summary>
    /// Метод добавляет вклад резистора в строку системы уравнений для конкретного контура.
    /// Если оба контура свободные, то метод корректно добавляет положительный вклад в диагональ
    /// и отрицательный – в перекрёстную позицию. Если один из контуров форсирован (значение задано),
    /// его вклад переносится в свободный член B.
    /// </summary>
    private void StampContribution(int m, int other, double coeff, double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state)
    {
        if (m == -1)
            return; // Если m = -1, то это земля – ничего не добавляем.
        if (freeMeshMap.ContainsKey(m))
        {
            var i = freeMeshMap[m];
            // Добавляем сопротивление в диагональный элемент:
            // Если ток I_m присутствует в уравнении, его вклад равен R * I_m.
            A[i, i] += coeff;
            if (other == -1)
            {
                // Если другой узел – земля, то ничего не вычитаем.
            }
            else if (freeMeshMap.ContainsKey(other))
            {
                // Если другой контур свободен, добавляем перекрёстный член: -R * I_other.
                var j = freeMeshMap[other];
                A[i, j] -= coeff;
            }
            else
            {
                // Если другой контур форсирован, переносим его вклад в свободный член.
                B[i] -= coeff * state.MeshCurrents[other];
            }
        }
    }
}