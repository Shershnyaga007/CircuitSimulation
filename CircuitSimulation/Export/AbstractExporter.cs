namespace CircuitSimulation.Export;

public abstract class AbstractExporter
{
    protected readonly string FilePath;

    protected readonly Dictionary<string, Dictionary<double, double>> ExportDictionary = new();
    
    protected AbstractExporter(string filePath)
    {
        FilePath = filePath;
    }

    public virtual void Add(string name, Dictionary<double, double> data)
    {
        ExportDictionary.Add(name, data);
    }
    
    public abstract void Export();
}