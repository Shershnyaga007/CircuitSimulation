namespace CircuitSimulation.Simulator.Components;

/// <summary>
/// Класс конденсатора.
/// Конденсатор определяется соотношением: I = C * dV/dt  ⇒  dV/dt = I/C.
/// Для обновления напряжения на конденсаторе используется явная схема (метод Эйлера):
///   V_new = V_old + dt * (I / C).
/// В данном случае разность токов (между двумя контурами) влияет на изменение напряжения.
/// При штамповке в систему уравнений конденсатор не вносит вклад в матрицу A,
/// а только добавляет постоянный член (текущую разность потенциалов) в B.
/// </summary>
public class Capacitor : CircuitComponent
{
    public double Capacitance; // Емкость
    public double Voltage; // Текущее напряжение на конденсаторе

    public Capacitor(int mesh1, int mesh2, double capacitance) : base(mesh1, mesh2)
    {
        Capacitance = capacitance;
        Voltage = 0; // по умолчанию 0 В; можем установить начальное значение вручную
    }

    public override void Stamp(double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state, double dt)
    {
        // При штамповке конденсатора мы фиксируем значение напряжения, которое накопилось на конденсаторе.
        // Если конденсатор соединяет два контура, то напряжение на одном вносится с положительным знаком,
        // а на другом – с отрицательным.
        StampConstant(Mesh1, Voltage, B, freeMeshMap);
        StampConstant(Mesh2, -Voltage, B, freeMeshMap);
    }

    /// <summary>
    /// Метод для добавления постоянного вклада в вектор свободных членов B.
    /// </summary>
    private void StampConstant(int mesh, double value, double[] B, Dictionary<int, int> freeMeshMap)
    {
        if (mesh == -1)
            return;
        if (freeMeshMap.ContainsKey(mesh))
        {
            B[freeMeshMap[mesh]] += value;
        }
    }

    /// <summary>
    /// Метод обновления напряжения на конденсаторе.
    /// Используется соотношение:
    ///   I = C * dV/dt  ⇒  dV/dt = I/C.
    /// В данной схеме дифференциальное уравнение аппроксимируется методом Эйлера:
    ///   V_new = V_old + dt * ((I[Mesh1] - I[Mesh2]) / C),
    /// где (I[Mesh1] - I[Mesh2]) – суммарный ток, протекающий через конденсатор.
    /// </summary>
    public void UpdateVoltage(SimulationState state, double dt)
    {
        var i1 = (Mesh1 == -1) ? 0 : state.MeshCurrents[Mesh1];
        var i2 = (Mesh2 == -1) ? 0 : state.MeshCurrents[Mesh2];
        Voltage += dt * ((i1 - i2) / Capacitance);
    }
}

