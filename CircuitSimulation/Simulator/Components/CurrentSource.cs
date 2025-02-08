namespace CircuitSimulation.Simulator.Components;

/// <summary>
/// Класс источника тока.
/// Источник задаёт ток по функции времени: I(t) = f(t).
/// Если один из выводов подключён к земле, то источник принудительно задаёт значение тока в данном контуре.
/// Если же источник подключён между двумя свободными контурами, его влияние в этой модели
/// обрабатывается отдельно (например, в методе Step класса Circuit).
/// </summary>
public class CurrentSource : CircuitComponent
{
    // Функция, задающая значение тока в зависимости от времени.
    public Func<double, double> CurrentFunction;

    public CurrentSource(int mesh1, int mesh2, Func<double, double> currentFunction) : base(mesh1, mesh2)
    {
        CurrentFunction = currentFunction;
    }

    public override void Stamp(double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state, double dt)
    {
        // Источники тока не штампуются в матрицу, если один из контактов – земля,
        // так как они напрямую задают значение тока в соответствующем контуре.
    }

    /// <summary>
    /// Метод применения источника тока.
    /// Если источник подключён между контуром и землёй, то соответствующий ток в
    /// состоянии симуляции (State.MeshCurrents) устанавливается равным значению, вычисленному функцией.
    /// Обратите внимание на знак – он определяется по полярности подключения.
    /// </summary>
    public void ApplySource(SimulationState state, double time)
    {
        var sourceValue = CurrentFunction(time);
        if (Mesh1 != -1 && Mesh2 == -1)
        {
            // Источник подключён к Mesh1, а Mesh2 – земля, поэтому Mesh1 получает значение источника.
            state.MeshCurrents[Mesh1] = sourceValue;
        }
        else if (Mesh2 != -1 && Mesh1 == -1)
        {
            // Источник подключён к Mesh2, а Mesh1 – земля, поэтому Mesh2 получает отрицательное значение.
            state.MeshCurrents[Mesh2] = -sourceValue;
        }
        // Если источник подключён между двумя свободными контурами,
        // его влияние должно быть учтено в формировании системы уравнений (не реализовано в данном примере).
        
        
    }
}

