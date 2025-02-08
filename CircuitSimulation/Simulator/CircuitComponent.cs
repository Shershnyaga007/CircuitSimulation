﻿namespace CircuitSimulation.Simulator;

/// <summary>
/// Абстрактный базовый класс для компонентов цепи.
/// Каждый компонент соединяет два контура (или один контур и «землю», которая обозначается значением -1).
/// В данной модели используется метод «штамповки» (stamping) – добавление вклада компонента
/// в систему линейных уравнений, описывающих динамику цепи.
/// </summary>
public abstract class CircuitComponent
{
    // Номера контуров, к которым подключён компонент.
    // Значение -1 означает «землю».
    public int Mesh1;
    public int Mesh2;

    /// <summary>
    /// Конструктор компонента.
    /// </summary>
    public CircuitComponent(int mesh1, int mesh2)
    {
        Mesh1 = mesh1;
        Mesh2 = mesh2;
    }

    /// <summary>
    /// Абстрактный метод штамповки компонента в матрицу системы уравнений.
    /// Здесь формируется вклад компонента в уравнения методом:
    ///   A * X = -B,
    /// где A – матрица коэффициентов, B – вектор свободных членов, а X – вектор неизвестных (контурные токи).
    /// freeMeshMap – словарь, отображающий номер свободного (нефорсированного) контура в индекс вектора X.
    /// state – текущее состояние симуляции (например, значения токов в контурах),
    /// dt – шаг по времени (используется для аппроксимации производных).
    /// </summary>
    public abstract void Stamp(double[,] A, double[] B, Dictionary<int, int> freeMeshMap, SimulationState state, double dt);
}
