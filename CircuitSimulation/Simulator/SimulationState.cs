namespace CircuitSimulation.Simulator;

/// <summary>
/// Класс, описывающий состояние симуляции.
/// Здесь хранятся текущие и предыдущие значения контурных токов, общее время симуляции и число контуров.
/// Сохранение предыдущего состояния необходимо для аппроксимации производных (например, для индуктивности).
/// </summary>
public class SimulationState
{
    public double[] MeshCurrents;
    public double[] PreviousMeshCurrents;
    public double Time;
    public int TotalMeshes;

    public SimulationState(int totalMeshes)
    {
        TotalMeshes = totalMeshes;
        MeshCurrents = new double[totalMeshes];
        PreviousMeshCurrents = new double[totalMeshes];
        Time = 0;
    }

    /// <summary>
    /// Метод копирования текущих значений токов в предыдущие.
    /// Это необходимо для расчёта приращений (например, для индуктивностей).
    /// </summary>
    public void SavePreviousState()
    {
        for (var i = 0; i < TotalMeshes; i++)
        {
            PreviousMeshCurrents[i] = MeshCurrents[i];
        }
    }
}

