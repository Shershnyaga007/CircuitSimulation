using CircuitSimulation.Simulator.CircuitBuilder;
using CircuitSimulation.Simulator.Components;

namespace CircuitSimulation.Examples;

/// <summary>
/// Модель зарядки конденастора от постоянного тока/
/// </summary>
public class DCCapacitorChargeExample : IExample
{
    public void Execute()
    {
        var circuit = new Circuit(0.1f);
        
        // Создаем источник постоянного тока с силой тока 1А.
        circuit.AddComponent(new CurrentSource(0, -1, _ => 1));
        circuit.AddComponent(new Resistor(0, 1, 10));
        circuit.AddComponent(new Capacitor(1, -1, 1));
        
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
            // Ищем конденсатор в списке компонентов и выводим напряжение на нём.
            foreach (var comp in circuit.Components)
            {
                if (comp is Capacitor cap)
                {
                    Console.WriteLine("Конденсатор (контуры {0} и {1}) Напряжение: {2:F3} В", cap.Mesh1, cap.Mesh2, cap.Voltage);
                }
            }
        }, state: null, dueTime: 0, period: 1);
        
        Console.WriteLine("Нажмите Enter для завершения симуляции...");
        Console.ReadLine();
    }
}