// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Sonny.Application.Presentation.Implements;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class AdjustStrBeamCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            IList<Reference> pickObjects = uidoc.Selection.PickObjects(ObjectType.Element, new BeamWallSelectionFilter()
                , "Select beams, walls and/or columns");

            var beams = new List<FamilyInstance>();
            var walls = new List<Wall>();
            // CHANGED: tách list cột.
            var columns = new List<FamilyInstance>();

            foreach (Reference pickObject in pickObjects)
            {
                var element = doc.GetElement(pickObject);
                if (element is Wall wall) { walls.Add(wall); }

                else if (element is FamilyInstance fi && fi.Category != null)
                {
#if ALB_R23 || ALB_R22 || ALB_R21
                    int cat = fi.Category.Id.IntegerValue;
#else
                    long cat = fi.Category.Id.Value;
#endif
                    if (cat == (int)BuiltInCategory.OST_StructuralFraming)
                        beams.Add(fi);
                    else if (cat == (int)BuiltInCategory.OST_StructuralColumns)
                        columns.Add(fi);
                }
            }

            if (!beams.Any() || (!walls.Any() && !columns.Any())) {
                TaskDialog.Show(
                    "Adjust Structural Beam",
                    "Please select at least one beam and one wall or column.");
                return Result.Cancelled;
            }

            var abViewModel = new AdjustStrBeamViewModel(uidoc, beams, walls, columns);
            var abWindow = new AdjustStrBeamWindow(abViewModel);
            if (abWindow.ShowDialog() != true) {
                return Result.Cancelled;
            }

            using (var transaction = new Transaction(doc, "Adjust Structural Beam Gap")) {
                transaction.Start();
                // CHANGED: truyền columns + 2 gap; Services ưu tiên tường nếu có.
                // CHANGED: thêm BeamBeamGapColumnMm (tổng gap G giữa 2 dầm trên cột).
                // CHANGED: thêm CutWidthLedgeAtPillar + BeamBeamPerpendicularGapMm (dầm thứ 3).
                AdjustStrBeamServices.Execute(doc, beams, walls, columns,
                    abViewModel.BeamGapWallMm,
                    abViewModel.BeamGapColumnMm,
                    abViewModel.BeamBeamGapColumnMm,
                    abViewModel.BeamBeamPerpendicularGapMm,
                    abViewModel.CutWidthLedgeAtPillar,
                    abViewModel.UseExteriorFace);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
}
