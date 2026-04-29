// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms.VisualStyles;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;


namespace SonnyBIM
{
    public class FloorServices
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly FloorFromCADViewModel _viewModel;
        private readonly IProgressReporter _reporter;
        private TransactionGroup _transG;
        string languageCode = LanguageData.GetLanguageSetting();
        private List<ElementId> _newFloors = new List<ElementId>();
        private List<ElementId> _newBoundaryLine = new List<ElementId>();

        public FloorServices(FloorFromCADViewModel viewModel, IProgressReporter reporter)
        {
            _viewModel = viewModel;
            _doc = viewModel.Doc;
            _uiDoc = viewModel.UiDoc;
            _reporter = reporter;
        }

        public void Execute()
        {
            List<Curve> allCurves = CadUtils.GetCurveHaveName(_viewModel.CadInstance, _viewModel.SelectedLayerFloor);
            if (_viewModel.OnlyCreateBoundaryLine) {
                using (_transG = new TransactionGroup(_doc)) {
                    _transG.Start("Run");
                    if (_transG.HasStarted()) {
                        int value = 0;
                        foreach (Curve curve in allCurves) {
                            value++;
                            _reporter.Update(value, allCurves.Count);
                            using (Transaction trans = new Transaction(_doc)) {
                                trans.Start("x");
                                FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                                trans.SetFailureHandlingOptions(failOpt);

                                try {
                                    ModelLine line = CurveUtils.CreateModelLine(_doc, curve);
                                    if (line != null) {
                                        _newBoundaryLine.Add(line.Id);
                                    }
                                }
                                catch (Exception) {
                                    continue;
                                }

                                trans.Commit();
                                _uiDoc.RefreshActiveView();
                            }
                        }
                    }

                    if (_transG.HasStarted()) {
                        _transG.Commit();
                        _reporter?.Close();
                        _newBoundaryLine = _newBoundaryLine.Where(id => id != null).ToList();
                        MessageBox.Show(
                            string.Concat("You have created ", _newBoundaryLine.Count, " Boundary Line of Floors!"),
                            SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _uiDoc.Selection.SetElementIds(_newBoundaryLine);
                    }
                }
            }
            else {
                // Lấy về nhóm những đường khép kín từ đường biên dạng
                // Trả về Empty khi người dùng nhập vào tên layer của Hatch floor
                List<List<Curve>> curveLoopsOfLines;
                using (Transaction tran = new Transaction(_doc, "x")) {
                    tran.Start();
                    curveLoopsOfLines = CurveUtils.GetCurveLoops(_doc, allCurves);
                    tran.RollBack();
                }

                // Lấy về tất cả face có hatch được chọn
                //  Trả về Empty khi người dùng nhập vào tên layer của đường biên dạng Floor
                List<PlanarFace> planarFace =
                    CadUtils.GetPlanarFaceHaveName(_viewModel.CadInstance, _viewModel.SelectedLayerFloor);

                if (curveLoopsOfLines.Any() && !_viewModel.CreateBoundaryLineOfHatch) {
                    using (_transG = new TransactionGroup(_doc)) {
                        _transG.Start("Run");
                        int value = 0;
                        foreach (List<Curve> curveContinue in curveLoopsOfLines) {
                            value++;
                            _reporter.Update(value, curveLoopsOfLines.Count);
                            if (_transG.HasStarted()) {
                                IList<CurveLoop> slabCurve = new List<CurveLoop>();
                                slabCurve.Add(CurveLoop.Create(curveContinue));
                                try {
                                    using (Transaction trans = new Transaction(_doc)) {
                                        trans.Start("x");
                                        FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                        failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                                        failOpt.SetClearAfterRollback(true);
                                        trans.SetFailureHandlingOptions(failOpt);

                                        Floor newFloor = Floor.Create(_doc, slabCurve, _viewModel.SelectedFloor.Id,
                                            _viewModel.SelectedLevel.Id);
                                        newFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM)
                                            .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.LevelOffset));
                                        _newFloors.Add(newFloor.Id);
                                        trans.Commit();
                                        _uiDoc.RefreshActiveView();
                                    }
                                }
                                catch (Exception) {
                                }
                            }
                            else {
                                break;
                            }
                        }

                        if (_transG.HasStarted()) {
                            _transG.Commit();
                            _reporter?.Close();
                            _newFloors.Where(id => id != null).ToList();

                            MessageBox.Show(string.Concat("You have created ", _newFloors.Count, " Floors!"),
                                SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _uiDoc.Selection.SetElementIds(_newFloors);
                        }
                    }
                }
                else if (planarFace.Any()) {
                    using (_transG = new TransactionGroup(_doc)) {
                        _transG.Start("Run");
                        if (_transG.HasStarted()) {
                            int value = 0;
                            foreach (PlanarFace hatch in planarFace) {
                                value++;
                                _reporter.Update(value, planarFace.Count);
                                IList<CurveLoop> curveLoops = hatch.GetEdgesAsCurveLoops();
                                foreach (CurveLoop cl in curveLoops) {
                                    List<CurveLoop> slabCurves = new List<CurveLoop>();
                                    slabCurves.Add(cl);

                                    using (Transaction trans = new Transaction(_viewModel.Doc)) {
                                        trans.Start("x");
                                        FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                                        failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                                        trans.SetFailureHandlingOptions(failOpt);

                                        if (_viewModel.CreateBoundaryLineOfHatch) {
                                            try {
                                                foreach (CurveLoop curveLoop in slabCurves) {
                                                    foreach (Curve curve in curveLoop) {
                                                        ModelLine line = CurveUtils.CreateModelLine(_doc, curve);
                                                        if (line != null) {
                                                            _newBoundaryLine.Add(line.Id);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception) {
                                            }
                                        }
                                        else {
                                            try {
                                                Floor newFloor = Floor.Create(_doc, slabCurves,
                                                    _viewModel.SelectedFloor.Id,
                                                    _viewModel.SelectedLevel.Id);
                                                newFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM)
                                                    .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.LevelOffset));
                                                _newFloors.Add(newFloor.Id);
                                            }
                                            catch (Exception) {
                                                continue;
                                            }
                                        }

                                        trans.Commit();
                                        _uiDoc.RefreshActiveView();
                                    }
                                }
                            }
                        }

                        if (_transG.HasStarted()) {
                            _transG.Commit();
                            _reporter?.Close();
                            if (_viewModel.CreateBoundaryLineOfHatch) {
                                _newBoundaryLine = _newBoundaryLine.Where(id => id != null).ToList();
                                MessageBox.Show(string.Concat("You have created ", _newBoundaryLine.Count,
                                        " Boundary Line of Floors!"), SonnyBIMConstraint.MessageBoxCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                _uiDoc.Selection.SetElementIds(_newBoundaryLine);
                            }
                            else {
                                _newFloors = _newFloors.Where(id => id != null).ToList();
                                MessageBox.Show(string.Concat("You have created ", _newFloors.Count,
                                        " Floors!"), SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                _uiDoc.Selection.SetElementIds(_newBoundaryLine);
                            }
                        }
                    }
                }
                else {
                    string text = BindingUtils.ChangeLanguage(languageCode,
                        "Kiểm tra lại các thông số đã nhập!",
                        "Double-check the entered parameters!");
                    MessageBox.Show(text, SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem CurveLoop cl có nằm hoàn toàn trong PlanarFace PFace hay không.
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="PFace"></param>
        /// <returns></returns>
        private bool CurveLoopIsInsideFace(CurveLoop cl, PlanarFace PFace)
        {
            if (cl.Count() == 0) return false;
            Transform Trans = PFace.ComputeDerivatives(new UV(0, 0));
            foreach (Curve cv in cl) {
                XYZ pt1 = Trans.Inverse.OfPoint(cv.GetEndPoint(0));
                bool outval = PFace.IsInside(new UV(pt1.X, pt1.Y), out IntersectionResult Res);
                if (!outval) {
                    return false;
                }
            }
            return true;
        }
    }
}
