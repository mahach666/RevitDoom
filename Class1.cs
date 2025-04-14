using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitDoom.Utils;
using System.Linq;

namespace RevitDoom
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var wadPath = UserSelect.GetWad();

            if (string.IsNullOrEmpty(wadPath)) return Result.Cancelled;

            var app = commandData.Application;
            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;

            var pixels = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(FilledRegion))
                .WhereElementIsNotElementType()
                .Cast<FilledRegion>()
                .OrderByDescending(r => GetCentral(r).Y)
                .ThenBy(r => GetCentral(r).X)
                .ToList();

            var builder = new AppBuilder();
            builder.SetIwad(wadPath)
                .EnableHighResolution(false)
                .WithArgs("-skill", "3")
                .WithScale(1)
                .WithPixels(pixels)
                .WithUIDocument(uidoc);

            var dapp = builder.Build();
            dapp.RunAsync();


            return Result.Succeeded;
        }
        private XYZ GetCentral(FilledRegion filledRegion)
        {
            var bb = filledRegion.get_BoundingBox(null);
            return (bb.Min + bb.Max) / 2;
        }
    }

}
