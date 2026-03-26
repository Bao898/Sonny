// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms.VisualStyles;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using MoreLinq;
using Sonny.Application.Domain.Services;
using Microsoft.Win32;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SonnyBIM;

public class WallServices
{
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private readonly WallFromCADViewModel _viewModel;
    private readonly IProgressReporter _reporter;

    private TransactionGroup _trans;
    private List<ElementId> _newWallIds = new List<ElementId>();
    private List<Wall> _newWalls = new List<Wall>();

    public WallServices(WallFromCADViewModel viewModel, IProgressReporter reporter)
    {
        _viewModel = viewModel;
        _doc = viewModel.Doc;
        _uiDoc = viewModel.UiDoc;
        _reporter = reporter;
    }

    public void Execute()
    {
        List<Curve> allLinesWall = new List<Curve>();
        List<PlanarFace> planarFaceToCreate =
            CadUtils.GetPlanarFaceHaveName(_viewModel.CadInstance, _viewModel.SelectedLayer);
        List<CurveLoop> allCurveLoop = new List<CurveLoop>();
        foreach (PlanarFace faceHatch in planarFaceToCreate) {
            allCurveLoop.AddRange(faceHatch.GetEdgesAsCurveLoops());
        }

        foreach (CurveLoop cvl in allCurveLoop) {
            foreach (Curve cv in cvl) {
                allLinesWall.Add(cv);
            }
        }

        // Loại bõ những line trùng nhau
        foreach (Curve cv in allLinesWall) {
            List<Curve> allLineLoopTocCheck = allLinesWall.Except(new[] { cv }).ToList();
            if (CurveUtils.IsInsideEntire(cv, allLineLoopTocCheck)) {
                allLinesWall = allLinesWall.Except(new[] { cv }).ToList();
            }
        }

        List<string> allThickness = new List<string>();
        List<double> allThicknessToCreate = new List<double>();

        // group lại thành từng cặp line song song
        List<List<Curve>> groupLine1 = new List<List<Curve>>();
        List<List<Curve>> groupLine2 = new List<List<Curve>>();
        List<List<Curve>> groupLine3 = new List<List<Curve>>();

        #region If for SelectedWallType1

        if (_viewModel.SelectedWallType1 != null) {
            if (_viewModel.IsEnabledThicknessType1 && !string.IsNullOrEmpty(_viewModel.AllThicknessType1)) {
                allThickness = _viewModel.AllThicknessType1.Split(Convert.ToChar(";")).ToList();
                allThicknessToCreate =
                    allThickness.Select(th => SonnyBIMUnitUtils.MmToFeet(Convert.ToDouble(th))).ToList();
                // Loại bõ những line có chiều dài bằng các bề rộng wall sẽ dựng
                foreach (var thick in allThicknessToCreate) {
                    allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - thick) > 0.001).ToList();
                }
            }
            else {
                allThicknessToCreate.Add(_viewModel.SelectedWallType1.Width);
                allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - _viewModel.SelectedWallType1.Width) > 0.001)
                    .ToList();
            }

            foreach (var thick in allThicknessToCreate) {
                groupLine1.AddRange(
                    CurveUtils.GetCurvesParallelAndDistance(allLinesWall.Select(l => l).ToList(), thick));
                // Loại bõ các đường line wall đã thuộc về groupLine1
                // Cách 1: Dùng Code
                //foreach (List<Curve> curves in groupLine1)
                //{
                //    foreach (Curve cv in curves)
                //        allLinesWall = allLinesWall.Except(new[] {cv}).ToList();
                //}
                // Cách 2: Dùng LinQ
                allLinesWall = groupLine1.SelectMany(curves => curves).Aggregate(allLinesWall, (current, cv)
                    => current.Except(new[] { cv }).ToList());
            }
        }

        #endregion If for SelectedWallType1

        #region If for SelectedWallType2

        if (_viewModel.SelectedWallType2 != null) {
            allThicknessToCreate = new List<double>();
            if (_viewModel.IsEnabledThicknessType2 && !string.IsNullOrEmpty(_viewModel.AllThicknessType2)) {
                allThickness = _viewModel.AllThicknessType2.Split(Convert.ToChar(";")).ToList();
                allThicknessToCreate =
                    allThickness.Select(th => SonnyBIMUnitUtils.MmToFeet(Convert.ToDouble(th))).ToList();
                // Loại bõ những line có chiều dài bằng các bề rộng wall sẽ dựng
                foreach (var thick in allThicknessToCreate) {
                    allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - thick) > 0.001).ToList();
                }
            }
            else {
                allThicknessToCreate.Add(_viewModel.SelectedWallType2.Width);
                allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - _viewModel.SelectedWallType2.Width) > 0.001)
                    .ToList();
            }

            foreach (var thick in allThicknessToCreate) {
                groupLine2.AddRange(
                    CurveUtils.GetCurvesParallelAndDistance(allLinesWall.Select(l => l).ToList(), thick));
                // Loại bõ các đường line wall đã thuộc về groupLine2
                allLinesWall = groupLine2.SelectMany(curves => curves).Aggregate(allLinesWall, (current, cv)
                    => current.Except(new[] { cv }).ToList());
            }
        }

        #endregion If for SelectedWallType2

        #region If for SelectedWallType3

        if (_viewModel.SelectedWallType3 != null) {
            allThicknessToCreate = new List<double>();
            if (_viewModel.IsEnabledThicknessType3 && !string.IsNullOrEmpty(_viewModel.AllThicknessType3)) {
                allThickness = _viewModel.AllThicknessType3.Split(Convert.ToChar(";")).ToList();
                allThicknessToCreate =
                    allThickness.Select(th => SonnyBIMUnitUtils.MmToFeet(Convert.ToDouble(th))).ToList();
                // Loại bõ những line có chiều dài bằng các bề rộng wall sẽ dựng
                foreach (var thick in allThicknessToCreate) {
                    allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - thick) > 0.001).ToList();
                }
            }
            else {
                allThicknessToCreate.Add(_viewModel.SelectedWallType3.Width);
                allLinesWall = allLinesWall.Where(l => Math.Abs(l.Length - _viewModel.SelectedWallType3.Width) > 0.001)
                    .ToList();
            }

            foreach (var thick in allThicknessToCreate) {
                groupLine3.AddRange(
                    CurveUtils.GetCurvesParallelAndDistance(allLinesWall.Select(l => l).ToList(), thick));
                // Loại bõ các đường line wall đã thuộc về groupLine3
                allLinesWall = groupLine3.SelectMany(curves => curves).Aggregate(allLinesWall, (current, cv)
                    => current.Except(new[] { cv }).ToList());
            }
        }

        #endregion If for SelectedWallType3

        _trans = new TransactionGroup(_doc);
        using (_trans) {

            #region Tạo wall type 1
            int valueType1 = 0;
            double value = 0;
            WallType? wallType = _viewModel.SelectedWallType1;

            if (!_trans.HasStarted()) {
                _trans.Start("Run");
            }

            foreach (List<Curve> lineWall in groupLine1) {
                if (_trans.HasStarted()) {
                    try {
                        double maxLength = lineWall.Max(c => c.Length);
                        Curve baseLine = lineWall.First(c => Math.Abs(c.Length - maxLength) < 0.001);
                        Curve secondLine = lineWall.Except(new[] { baseLine }).First();
                        XYZ orientation = baseLine.NormalParallelTo(secondLine);

                        using (Transaction trans = new Transaction(_doc)) {
                            trans.Start("x");
                            FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                            trans.SetFailureHandlingOptions(failOpt);

                            Wall wall = Wall.Create(_doc, baseLine, _viewModel.BaseLevel.Id,
                                _viewModel.IsCreateWallStructural);
                            wall.WallType = wallType;
                            wall.Location.Move(orientation * wall.Width / 2);
                            wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(4);
                            wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(_viewModel.TopLevel.Id);
                            wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.TopOffset));
                            wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.BaseOffset));

                            _newWallIds.Add(wall.Id);
                            _newWalls.Add(wall);
                            trans.Commit();
                        }
                        valueType1++;
                        _reporter.Update(valueType1, groupLine1.Count);
                    }
                    catch (Exception) {
                    }
                }
                else {
                    break;
                }
            }
            #endregion Tạo wall type 1

            #region Tạo wall type 2
            int valueType2 = 0;
            value = 0;
            wallType = _viewModel.SelectedWallType2;

            if (!_trans.HasStarted()) {
                _trans.Start("Run");
            }

            foreach (List<Curve> lineWall in groupLine2) {
                if (_trans.HasStarted()) {
                    try {
                        double maxLength = lineWall.Max(c => c.Length);
                        Curve baseLine = lineWall.First(c => Math.Abs(c.Length - maxLength) < 0.001);
                        Curve secondLine = lineWall.Except(new[] { baseLine }).First();
                        XYZ orientation = baseLine.NormalParallelTo(secondLine);

                        using (Transaction trans = new Transaction(_doc)) {
                            trans.Start("x");
                            FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                            trans.SetFailureHandlingOptions(failOpt);

                            Wall wall = Wall.Create(_doc, baseLine, _viewModel.BaseLevel.Id,
                                _viewModel.IsCreateWallStructural);
                            wall.WallType = wallType;
                            wall.Location.Move(orientation * wall.Width / 2);
                            wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(4);
                            wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(_viewModel.TopLevel.Id);
                            wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.TopOffset));
                            wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.BaseOffset));

                            _newWallIds.Add(wall.Id);
                            _newWalls.Add(wall);
                            trans.Commit();
                        }
                        valueType2++;
                        _reporter.Update(valueType2, groupLine2.Count);
                    }
                    catch (Exception) {
                    }
                }
                else {
                    break;
                }
            }
            #endregion Tạo wall type 2

            #region Tạo wall type 3
            int valueType3 = 0;
            value = 0;
            wallType = _viewModel.SelectedWallType3;

            if (!_trans.HasStarted()) {
                _trans.Start("Run");
            }

            foreach (List<Curve> lineWall in groupLine3) {
                if (_trans.HasStarted()) {
                    try {
                        double maxLength = lineWall.Max(c => c.Length);
                        Curve baseLine = lineWall.First(c => Math.Abs(c.Length - maxLength) < 0.001);
                        Curve secondLine = lineWall.Except(new[] { baseLine }).First();
                        XYZ orientation = baseLine.NormalParallelTo(secondLine);

                        using (Transaction trans = new Transaction(_doc)) {
                            trans.Start("x");
                            FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                            trans.SetFailureHandlingOptions(failOpt);

                            Wall wall = Wall.Create(_doc, baseLine, _viewModel.BaseLevel.Id,
                                _viewModel.IsCreateWallStructural);
                            wall.WallType = wallType;
                            wall.Location.Move(orientation * wall.Width / 2);
                            wall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set(4);
                            wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(_viewModel.TopLevel.Id);
                            wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.TopOffset));
                            wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.BaseOffset));

                            _newWallIds.Add(wall.Id);
                            _newWalls.Add(wall);
                            trans.Commit();
                        }
                        valueType3++;
                        _reporter.Update(valueType3, groupLine3.Count);
                    }
                    catch (Exception) {
                    }
                }
                else {
                    break;
                }
            }
            #endregion Tạo wall type 3

            using (Transaction trans = new Transaction(_doc)) {
                trans.Start("x");
                FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                trans.SetFailureHandlingOptions(failOpt);

                foreach (var w in _newWalls) {
                    IList<Element> elements = w.GetWallsBoundingBoxIntersectWith(_doc, false, true, _newWallIds, false);
                    foreach (var otherWall in elements) {
                        if (!JoinGeometryUtils.AreElementsJoined(_doc, w, otherWall)) {
                            try {
                                JoinGeometryUtils.JoinGeometry(_doc, w, otherWall);
                            }
                            catch (Exception ) {
                                continue;
                            }
                        }
                    }
                }
                trans.Commit();
            }

            if (_trans.HasStarted()) {
                _trans.Commit();

                _newWallIds = _newWallIds.Where(id => id != null).ToList();

                MessageBox.Show(string.Concat("You have created ", _newWallIds.Count,
                    " Walls!"), SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                _uiDoc.Selection.SetElementIds(_newWallIds);
            }
        }
    }
}
