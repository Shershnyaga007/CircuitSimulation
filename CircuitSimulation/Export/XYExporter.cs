namespace CircuitSimulation.Export;

public class XYExporter : AbstractExporter
{
    
    public XYExporter(string filePath) : base(filePath)
    {
        
    }

    public override void Export()
    {
        var dir = new DirectoryInfo(FilePath);

        if (!dir.Exists)
        {
            dir.Create();
        }
        
        foreach (var k in ExportDictionary)
        {
            using var fileStream = new FileStream(FilePath + k.Key + ".xy", FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(fileStream);
            
            foreach (var info in k.Value)
            {
                writer.Write($"{info.Key} {info.Value}\n");
            }
        }
    }
}