// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;

namespace SonnyBIM;

public class FoundationServices
{
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private readonly IProgressReporter _reporter;

    private readonly FoundationFromCADViewModel _viewModel;
    private List<Curve> _allLineToCreate = new List<Curve>();
    private List<ElementId> _numFloorsCreate = new List<ElementId>();
    private TransactionGroup _transG;

    public FoundationServices(FoundationFromCADViewModel viewModel, IProgressReporter reporter)
    {
        _viewModel = viewModel;
        _doc = viewModel.Doc;
        _uiDoc = viewModel.UiDoc;
        _reporter = reporter;
        _transG = new TransactionGroup(_viewModel.Doc);

    }

    public void Execute()
    {
        _allLineToCreate = CadUtils.GetCurveHaveName(_viewModel.CadInstance, _viewModel.SelectedLayer);
        List<List<Curve>> curveLoops;
        using (Transaction tran = new Transaction(_doc,"x")) {
            tran.Start();
            curveLoops = CurveUtils.GetCurveLoops(_doc, _allLineToCreate);
            tran.RollBack();
        }

        if (curveLoops.Count == 0) return;
        int value = 0;

        _transG.Start("Run");
        foreach (List<Curve> curveContinue in curveLoops) {
            IList<CurveLoop> slabCurves = new List<CurveLoop>();
            slabCurves.Add(CurveLoop.Create(curveContinue));

            if (_transG.HasStarted()) {
                value++;
                _reporter.Update(value, curveLoops.Count);

                try {
                    using (Transaction trans = new Transaction(_doc, "x")) {
                        trans.Start();
                        FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                        failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                        failOpt.SetClearAfterRollback(true);
                        trans.SetFailureHandlingOptions(failOpt);

                        Floor newFloor = Floor.Create(_doc, slabCurves, _viewModel.SelectedFloor.Id, _viewModel.SelectedLevel.Id);
                        newFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM)
                            .Set(_viewModel.OffsetFromElevationToRevitUnit);
                        _numFloorsCreate.Add(newFloor.Id);

                        trans.Commit();
                    }
                }
                catch (Exception ) {

                }
            }
            else {
                break;
            }
        }

        if (_transG.HasStarted()) {
            _transG.Commit();
            _reporter.Close();

            _numFloorsCreate = _numFloorsCreate.Where(id => id != null).ToList();
            MessageBox.Show(string.Concat("You have created ", _numFloorsCreate.Count, " Slab Foundation")
                ,SonnyBIMConstraint.MessageBoxCaption,MessageBoxButtons.OK,MessageBoxIcon.Information);
            _uiDoc.Selection.SetElementIds(_numFloorsCreate);
        }

    }

}
