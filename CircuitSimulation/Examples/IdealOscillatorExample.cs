using CircuitSimulation.Simulator.CircuitBuilder;
using CircuitSimulation.Simulator.Components;

namespace CircuitSimulation.Examples;

/// <summary>
/// Пример симуляции идеального LC-осциллятора.
/// LC-осциллятор описывается дифференциальными уравнениями:
///   V = L * dI/dt   и   I = C * dV/dt,
/// где V – напряжение на конденсаторе, I – ток через индуктивность.
/// При дискретизации по времени (шаг dt) получаем аппроксимации:
///   I(n+1) = I(n) - (dt/L) * V(n)
///   V(n+1) = V(n) + (dt/C) * I(n+1)
/// В данной реализации для упрощения модели используется два контура:
/// - Mesh0: свободный контур, динамика которого моделируется по уравнениям.
/// - Mesh1: форсированный контур, ток в котором принудительно равен 0 с помощью источника тока.
/// Элементы LC (индуктивность и конденсатор) подключаются между Mesh0 и Mesh1.
/// Начальное напряжение на конденсаторе задаётся отличным от нуля (например, 1 В),
/// что запускает процесс обмена энергии между магнитным полем индуктора и электрическим полем конденсатора.
/// </summary>
public class IdealOscillatorExample : IExample
{
    public void Execute()
    {
        const double dt = 0.01;
        var circuit = new Circuit(dt);

        /*
         Для моделирования идеального LC-осциллятора используем два контура:
         - Mesh0: свободный (динамический) контур.
         - Mesh1: форсированный нулевым током (подключён к земле через токовый источник с I = 0).
         Элементы LC (индуктивность и конденсатор) подключаются между Mesh0 и Mesh1.

         Основные формулы дискретизации:
         
         Для индуктивности (закон Фарадея):
             V_L = L * (dI/dt)
         При схеме обратного Эйлера:
             I(n+1) = I(n) + (dt/L) * V_L(n+1)
         Перенеся V_L(n+1) в правую часть, получаем:
             I(n+1) = I(n) - (dt/L) * V_C(n)
         (учитывая, что V_L = -V_C, если рассматривать цепь замкнутой)

         Для конденсатора (закон зарядов):
             I_C = C * (dV/dt)
         При дискретизации:
             V(n+1) = V(n) + (dt/C) * I(n+1)
         
         Эти соотношения отражают обмен энергии между индуктивностью и конденсатором,
         что приводит к периодическим колебаниям.
        */

        // Форсируем Mesh1 (контур с индексом 1) равным 0 А с помощью источника тока.
        circuit.AddComponent(new CurrentSource(1, -1, t => 0.0));

        const double L = 1.0;
        const double C = 1.0;

        // Подключаем индуктивность между Mesh0 и Mesh1.
        circuit.AddComponent(new Inductor(0, 1, L));

        // Подключаем конденсатор между Mesh0 и Mesh1.
        var cap = new Capacitor(0, 1, C)
        {
            Voltage = 1.0 // Задаём начальное напряжение 1 В, что запускает колебания.
        };
        circuit.AddComponent(cap);

        // Инициализируем состояние симуляции.
        circuit.InitializeState();

        // Устанавливаем начальные значения токов в контурах.
        // Mesh0 – свободный (динамический) контур; Mesh1 – форсированный (0 А).
        circuit.State.MeshCurrents[0] = 0.0;
        circuit.State.MeshCurrents[1] = 0.0;
        circuit.State.PreviousMeshCurrents[0] = 0.0;
        circuit.State.PreviousMeshCurrents[1] = 0.0;

        // Для имитации работы симуляции в реальном времени используем класс Timer.
        var timer = new Timer(_ =>
        {
            circuit.Step();
            Console.Clear();
            Console.WriteLine("Время: {0:F3} с", circuit.State.Time);
            Console.WriteLine("Mesh0 (свободный) ток: {0:F6} А", circuit.State.MeshCurrents[0]);
            Console.WriteLine("Mesh1 (форсированный, 0 А) ток: {0:F6} А", circuit.State.MeshCurrents[1]);
            Console.WriteLine("Напряжение на конденсаторе (между Mesh0 и Mesh1): {0:F6} В", cap.Voltage);
        }, state: null, dueTime: 0, period: 1);
        
        Console.WriteLine("Нажмите Enter для завершения симуляции...");
        Console.ReadLine();
    }
}