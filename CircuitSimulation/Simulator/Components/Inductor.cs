namespace CircuitSimulation.Simulator.Components;

/// <summary>
/// Класс катушки индуктивности.
/// Индуктивность описывается уравнением: V = L * dI/dt.
/// Для численной симуляции используется схема обратного Эйлера:
///   dI/dt ≈ (I_new - I_old) / dt,
/// откуда получаем:
///   V ≈ L/dt * (I_new - I_old).
/// При штамповке в матрицу A добавляется коэффициент L/dt, а вклад из прошлой итерации
/// (L/dt * I_old) переносится в свободный член B.
/// </summary>
public class Inductor : CircuitComponent
{
    public double Inductance;

    public Inductor(int mesh1, int mesh2, double inductance) : base(mesh1, mesh2)
    {
        Inductance = inductance;
    }

    public override void Stamp(double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state, double dt)
    {
        // Коэффициент для индуктивности L/dt.
        var coeff = Inductance / dt;
        // Получаем предыдущие значения токов для обоих контуров.
        var prev_m1 = (Mesh1 == -1) ? 0 : state.PreviousMeshCurrents[Mesh1];
        var prev_m2 = (Mesh2 == -1) ? 0 : state.PreviousMeshCurrents[Mesh2];
        // Постоянный член, возникающий из аппроксимации производной:
        // constantTerm = - (L/dt) * (I_prev_mesh1 - I_prev_mesh2)
        var constantTerm = -coeff * (prev_m1 - prev_m2);
        // Штампуем вклад индуктивности для обоих контуров.
        StampContribution(Mesh1, Mesh2, coeff, constantTerm, A, B, freeMeshMap, state);
        StampContribution(Mesh2, Mesh1, coeff, -constantTerm, A, B, freeMeshMap, state);
    }

    /// <summary>
    /// Метод штамповки для индуктивности. Для каждого из подключённых контуров:
    /// - Добавляем L/dt в диагональный элемент.
    /// - Если второй контур свободен, то добавляем перекрёстный элемент -L/dt.
    /// - Переносим постоянный член, связанный с предыдущим состоянием, в вектор свободных членов B.
    /// </summary>
    private void StampContribution(int m, int other, double coeff, double constantTerm, double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state)
    {
        if (m == -1)
            return;
        if (freeMeshMap.ContainsKey(m))
        {
            var i = freeMeshMap[m];
            A[i, i] += coeff;
            if (other != -1 && freeMeshMap.ContainsKey(other))
            {
                var j = freeMeshMap[other];
                A[i, j] -= coeff;
            }
            else if (other != -1)
            {
                // Если второй контур форсирован, его вклад переносим в вектор B.
                B[i] -= coeff * state.MeshCurrents[other];
            }
            // Добавляем постоянный член, обусловленный предыдущим состоянием индуктора.
            B[i] += constantTerm;
        }
    }
}
