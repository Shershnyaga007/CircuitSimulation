using System.Collections;
using System.Net.NetworkInformation;
using ScottPlot;

namespace CircuitSimulation.Export;

public class PngExporter : AbstractExporter
{
    public Plot Plt { get; set; }

    private int width;
    private int height;
    
    public PngExporter(string filePath, int width, int height) : base(filePath)
    {
        Plt = new Plot();
        this.width = width;
        this.height = height;
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
            var x = new ArrayList();
            var y = new ArrayList();
            
            foreach (var info in k.Value)
            {
                x.Add(info.Key);
                y.Add(info.Value);
            }
            
            Plt.Add.Scatter(x.ToArray(), y.ToArray());
            Plt.SavePng(FilePath + k.Key + ".png", width, height);
        }
    }
}