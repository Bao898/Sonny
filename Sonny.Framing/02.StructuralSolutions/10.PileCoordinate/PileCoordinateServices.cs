// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.DB.Lighting;
using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;
using Transaction = System.Transactions.Transaction;

namespace SonnyBIM;

public class PileCoordinateServices
{
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private readonly PileCoordinateViewModel _viewModel;
    private TransactionGroup _transG;
    private Transaction _trans;
    private List<ElementId> _elementSuccess = new List<ElementId>();
    private readonly IProgressReporter _reporter;

    public PileCoordinateServices(PileCoordinateViewModel viewModel, IProgressReporter reporter)
    {
        _viewModel = viewModel;
        _doc = viewModel.Doc;
        _uiDoc = viewModel.UiDoc;
        _reporter = reporter;
    }

    public void Execute()
    {
        if (_viewModel.IsProjectBasePoint) {
            double versionNumber = Convert.ToDouble(_doc.Application.VersionNumber);
            if (versionNumber < Convert.ToDouble("2020")) {
                MessageBox.Show("Get coordinate according to Project Base Point " +
                                "only available for Revit from 2020.", SonnyBIMConstraint.MessageBoxCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        ProjectLocation projectLocation = _doc.ActiveProjectLocation;
        ProjectPosition projectPosition = projectLocation.GetProjectPosition(new XYZ());

        double projectAngle = projectPosition.Angle;
        double projectX = projectPosition.EastWest;
        double projectY = projectPosition.NorthSouth;

        List<ElementFilter> filters = new List<ElementFilter>();
        filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation));
        filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns));
        LogicalOrFilter orFilter = new LogicalOrFilter(filters);

        if (_viewModel.IsCurrentSelection) {
            List<ElementId> currentSelection = _uiDoc.Selection.GetElementIds().ToList();
            if (currentSelection.Count == 0) {
                MessageBox.Show("Select Piles Foundation or Structural Column before run this plugin!",
                    SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK);
                return;
            }
            _viewModel.SelectedElements = new FilteredElementCollector(_doc, currentSelection)
                .WherePasses(orFilter).ToList();
        }
        else if (_viewModel.IsCurrentView) {
            _viewModel.SelectedElements = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .WherePasses(orFilter).ToList();
        }
        else {
            _viewModel.SelectedElements = new FilteredElementCollector(_doc)
                .WherePasses(orFilter).ToList();
        }

        _transG = new TransactionGroup(_doc);
        using (_transG) {
            _transG.Start("Run");
            foreach (Element pile in _viewModel.SelectedElements) {
                if (_transG.HasStarted()) {
                    // GetCoordinate
                }
            }
        }
    }
}
