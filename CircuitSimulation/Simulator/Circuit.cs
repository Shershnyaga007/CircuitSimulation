using CircuitSimulation.Simulator.Components;

namespace CircuitSimulation.Simulator.CircuitBuilder;

/// <summary>
/// Основной класс цепи.
/// Здесь собираются компоненты, формируется система уравнений, выполняется шаг симуляции.
/// Метод Step реализует численное решение системы методом конечных разностей.
/// </summary>
public class Circuit
{
    public List<CircuitComponent> Components;
    public SimulationState State;
    public double TimeStep;
    public int TotalMeshes;

    public Circuit(double timeStep)
    {
        Components = new List<CircuitComponent>();
        TimeStep = timeStep;
        TotalMeshes = 0;
    }

    /// <summary>
    /// Добавление компонента в цепь.
    /// При добавлении компонента обновляется общее число контуров (если номер контура больше текущего).
    /// </summary>
    public void AddComponent(CircuitComponent comp)
    {
        Components.Add(comp);
        if (comp.Mesh1 > TotalMeshes - 1)
            TotalMeshes = comp.Mesh1 + 1;
        if (comp.Mesh2 > TotalMeshes - 1)
            TotalMeshes = comp.Mesh2 + 1;
    }

    /// <summary>
    /// Инициализация состояния симуляции.
    /// Создаётся объект SimulationState с заданным числом контуров.
    /// </summary>
    public void InitializeState()
    {
        State = new SimulationState(TotalMeshes);
    }

    /// <summary>
    /// Метод, выполняющий один шаг симуляции.
    /// Последовательность действий:
    /// 1. Сохраняем предыдущее состояние (для расчёта производных).
    /// 2. Применяем источники тока, подключённые к земле, которые форсируют значение контура.
    /// 3. Определяем «свободные» контуры – те, значение которых не задано напрямую.
    /// 4. Формируем систему уравнений методом штамповки (Stamp) для всех компонентов, кроме источников тока.
    /// 5. Решаем полученную систему уравнений (методом Гаусса).
    /// 6. Обновляем значения токов в свободных контурах.
    /// 7. Обновляем напряжения на конденсаторах (интегрируя ток по времени).
    /// 8. Увеличиваем время симуляции на dt.
    /// </summary>
    public void Step()
    {
        // Сохраняем предыдущее состояние для вычислений (например, для индуктивности).
        State.SavePreviousState();

        // Применяем источники тока, подключённые к земле.
        // Такие источники форсируют значение тока в соответствующем контуре.
        foreach (var comp in Components)
        {
            if (comp is CurrentSource cs)
            {
                if ((cs.Mesh1 != -1 && cs.Mesh2 == -1) ||
                    (cs.Mesh2 != -1 && cs.Mesh1 == -1))
                {
                    cs.ApplySource(State, State.Time);
                }
            }
        }

        // Определяем свободные контуры (те, значения которых не форсированы источниками тока).
        var freeMeshMap = new Dictionary<int, int>();
        var forced = new bool[State.TotalMeshes];
        
        // Отмечаем те контуры, где значение тока уже задано через источник.
        foreach (var comp in Components)
        {
            if (comp is CurrentSource cs)
            {
                if (cs.Mesh1 != -1 && cs.Mesh2 == -1)
                    forced[cs.Mesh1] = true;
                else if (cs.Mesh2 != -1 && cs.Mesh1 == -1)
                    forced[cs.Mesh2] = true;
            }
        }
        var freeIndex = 0;
        for (var m = 0; m < State.TotalMeshes; m++)
        {
            if (!forced[m])
            {
                freeMeshMap[m] = freeIndex;
                freeIndex++;
            }
        }
        var nFree = freeMeshMap.Count;

        // Инициализируем матрицу A и вектор B для системы уравнений.
        // Размер системы равен числу свободных контуров.
        var A = new double[nFree, nFree];
        var B = new double[nFree];

        // Штампуем все компоненты, кроме источников тока (они уже задали свои значения).
        foreach (var comp in Components)
        {
            if (comp is CurrentSource)
                continue;
            comp.Stamp(A, B, freeMeshMap, State, TimeStep);
        }

        // Решаем систему линейных уравнений: A * X = -B.
        // Обратите внимание, что свободный член переносится с обратным знаком.
        var X = SolveLinearSystem(A, B, nFree);

        // Обновляем значения токов для свободных контуров.
        foreach (var kv in freeMeshMap)
        {
            var mesh = kv.Key;
            var index = kv.Value;
            State.MeshCurrents[mesh] = X[index];
        }

        // Обновляем напряжения на конденсаторах, интегрируя ток по времени.
        foreach (var comp in Components)
        {
            if (comp is Capacitor cap)
            {
                cap.UpdateVoltage(State, TimeStep);
            }
        }

        // Увеличиваем время симуляции.
        State.Time += TimeStep;
    }

    /// <summary>
    /// Метод решения системы линейных уравнений методом Гаусса.
    /// Решаем систему: A * X = -B.
    /// Здесь копируем матрицы, нормализуем строки, выполняем прямой и обратный ход.
    /// </summary>
    private double[] SolveLinearSystem(double[,] A, double[] B, int n)
    {
        // Копируем матрицу и вектор, меняя знак свободного члена.
        var a = new double[n, n];
        var b = new double[n];
        for (var i = 0; i < n; i++)
        {
            b[i] = -B[i];
            for (var j = 0; j < n; j++)
            {
                a[i, j] = A[i, j];
            }
        }

        // Прямой ход метода Гаусса.
        for (var i = 0; i < n; i++)
        {
            var pivot = a[i, i];
            if (Math.Abs(pivot) < 1e-9)
                throw new Exception("Сингулярная матрица в симуляции.");
            for (var j = i; j < n; j++)
                a[i, j] /= pivot;
            b[i] /= pivot;
            for (var k = i + 1; k < n; k++)
            {
                var factor = a[k, i];
                for (var j = i; j < n; j++)
                    a[k, j] -= factor * a[i, j];
                b[k] -= factor * b[i];
            }
        }

        // Обратный ход метода Гаусса.
        var x = new double[n];
        for (var i = n - 1; i >= 0; i--)
        {
            x[i] = b[i];
            for (var j = i + 1; j < n; j++)
                x[i] -= a[i, j] * x[j];
        }
        return x;
    }
}

