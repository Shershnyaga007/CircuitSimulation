using CircuitSimulation.Simulator.CircuitBuilder;
using CircuitSimulation.Simulator.Components;

namespace CircuitSimulation.Examples;

/// <summary>
/// Пример симуляции работы катушки индуктивности в AC токе.
/// </summary>
public class ACExample : IExample
{
    public void Execute()
    {
        var circuit = new Circuit(0.0001f);

        // Создаем источник тока и описываем его силу тока в зависимости от времени.
        // В данном случае создадим симуляцию АС тока с частотой 60гц и I_max равной 1 ампер.
        var source = new CurrentSource(0, -1, time => Math.Sin(2 * Math.PI * time * 60));
        
        circuit.AddComponent(source);
        circuit.AddComponent(new Resistor(0, 1, 10));
        circuit.AddComponent(new Inductor(1, -1, 1));
        
        circuit.InitializeState();

        var timer = new Timer(_ =>
        {
            // Выполняем один шаг симуляции.
            circuit.Step();
            Console.Clear();
            Console.WriteLine("Время: {0:F3} с", circuit.State.Time);
            for (var m = 0; m < circuit.State.TotalMeshes; m++)
            {
                Console.WriteLine("Контур {0} Ток: {1:F3} А", m, circuit.State.MeshCurrents[m]);
            }
        }, state: null, dueTime: 0, period: 1);
        
        Console.WriteLine("Нажмите Enter для завершения симуляции...");
        Console.ReadLine();
    }
}