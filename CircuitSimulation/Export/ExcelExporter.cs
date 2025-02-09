using OfficeOpenXml;

namespace CircuitSimulation.Export;

public class ExcelExporter : AbstractExporter
{

    public ExcelExporter(string filePath) : base(filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public override void Export()
    {
        using var excelPackage = new ExcelPackage();
        
        foreach (var v in ExportDictionary)
        {
            var worksheet = excelPackage.Workbook.Worksheets.Add(v.Key);
            worksheet.Cells["A1"].LoadFromCollection(v.Value);
        }
            
        excelPackage.SaveAs(new FileInfo(FilePath));
    }
}