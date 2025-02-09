using CircuitSimulation.Export;
using CircuitSimulation.Simulator.CircuitBuilder;
using CircuitSimulation.Simulator.Components;

namespace CircuitSimulation.Examples;

public class ExportExample : IExample
{
    public void Execute()
    {
        var circuit = new Circuit(0.0001f);

        var capacitor = new Capacitor(1, -1, 1f);
            
        circuit.AddComponent(new CurrentSource(0, -1, _ => 1));
        circuit.AddComponent(new Resistor(0, 1, 10));
        circuit.AddComponent(capacitor);
            
        circuit.InitializeState();

        var XYExporter = new XYExporter("CapacitorVoltage/");
        var ExcelExporter = new ExcelExporter("CapacitorVoltage/CapacitorVoltage.xlsx");
        var pngExporter = new PngExporter("CapacitorVoltage/", 1000, 1000);
            
        pngExporter.Plt.Title("Capacitor Voltage");
        pngExporter.Plt.XLabel("Time");
        pngExporter.Plt.YLabel("Voltage");
            
        var voltages = new Dictionary<double, double>();
        while (circuit.State.Time <= 30)
        {
            circuit.Step();
            voltages.Add(circuit.State.Time, capacitor.Voltage);
        }
            
        pngExporter.Add("Capacitor", voltages);
        pngExporter.Export();
            
        XYExporter.Add("Capacitor", voltages);
        XYExporter.Export();
            
        ExcelExporter.Add("Capacitor", voltages);
        ExcelExporter.Export();
    }
}