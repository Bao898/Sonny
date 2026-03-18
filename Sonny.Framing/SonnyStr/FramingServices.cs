// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;
using Sonny.RevitExtensions.Extensions.GeometryObjects.Curves;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;


namespace SonnyBIM
{
    public class FramingServices
    {
        private readonly Document _doc;
        private readonly FramingFromCADViewModel _viewModel;
        private readonly IProgressReporter _reporter;

        // Các biến private bê y nguyên từ class cũ qua
        private List<Curve> _allLinesFraming = new List<Curve>();
        private List<double> _rongDams = new List<double>();
        private FamilySymbol _firstFamilySymbol = null;
        private List<ElementId> _newBeamsIds = new List<ElementId>();
        private Dictionary<FamilyInstance, XYZ> _newBeamsCouper = new Dictionary<FamilyInstance, XYZ>();
        private Transaction _trans;

        public FramingServices(FramingFromCADViewModel viewModel, IProgressReporter reporter)
        {
            _viewModel = viewModel;
            _doc = viewModel.Doc;
            _reporter = reporter;
        }

        public void Execute()
        {
            // --- BẮT ĐẦU LOGIC BÊ Y NGUYÊN TỪ HÀM PROCESS ---
            List<FamilySymbol> allFamilySymbol = _viewModel.SelectedFamilyFraming.GetAllFamilySymbol();
            if (!allFamilySymbol.Any()) return;

            List<string> listSections = _viewModel.AllSections.Split(Convert.ToChar(";")).ToList();
            if (!listSections.Any()) return;

            listSections = listSections.Select(s => s.Trim()).ToList();
            _allLinesFraming = CadUtils.GetLineHaveName(_viewModel.CadInstance, _viewModel.SelectedLayer);

            #region Add Code To Fix Bug
            // 2. Hoặc gom thành List chứa chuỗi tọa độ để bạn dễ đọc trong cửa sổ Watch
            List<string> debugPoints = _allLinesFraming.Select(c =>
                $"L={Math.Round(c.Length, 4)} | Start: {c.GetEndPoint(0)} | End: {c.GetEndPoint(1)}").ToList();
            #endregion

            double rongDam = 0;
            double caoDam = 0;
            _trans = new Transaction(_doc, "Framing From Cad");

            using (_trans) {
                _trans.Start("Run");

                WarningDeleteWarning deleteWarning = new WarningDeleteWarning();
                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(deleteWarning);
                _trans.SetFailureHandlingOptions(failOpt);

                foreach (string section in listSections) {
                    if (string.IsNullOrEmpty(section)) continue;

                    char[] separator = new[] { Convert.ToChar("x"), Convert.ToChar("X") };
                    string[] sec = section.Split(separator);

                    try {
                        rongDam = Convert.ToDouble(sec[0]);
                        caoDam = Convert.ToDouble(sec[1]);
                    }
                    catch (Exception) {
                        continue;
                    }

                    if (rongDam <= 0 || caoDam <= 0) continue;

                    _rongDams.Add(SonnyBIMUnitUtils.MmToFeet(rongDam));
                    List<List<Curve>> listFramingWithSection = CurveUtils.GetCurvesParallelAndDistance(_allLinesFraming,
                        SonnyBIMUnitUtils.MmToFeet(rongDam));

                    #region Add Code To Fix Bug
                    // 1. Tạo một danh sách ẩn danh để xem trong cửa sổ Watch khi Debug
                    var debugPairings = listFramingWithSection.Select((group, index) => new {
                        BeamIndex = index,
                        // Đường chuẩn (LineA)
                        LineA_Start = group[0].GetEndPoint(0).ToString(),
                        LineA_End = group[0].GetEndPoint(1).ToString(),
                        LineA_LengthMm = Math.Round(SonnyBIMUnitUtils.FeetToMm(group[0].Length), 0),

                        // Các đường đối diện tìm được (LineB)
                        OppositeLines = group.Skip(1).Select(c => new {
                            LineB_Start = c.GetEndPoint(0).ToString(),
                            LineB_End = c.GetEndPoint(1).ToString(),
                            LineB_LengthMm = Math.Round(SonnyBIMUnitUtils.FeetToMm(c.Length), 0)
                        }).ToList()
                    }).ToList();

                    // 2. Nếu bạn muốn in ra màn hình Output để copy cho dễ
                    foreach (var item in debugPairings)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dầm {item.BeamIndex}:");
                        System.Diagnostics.Debug.WriteLine($" - Line A: {item.LineA_Start} to {item.LineA_End} (L={item.LineA_LengthMm})");
                        foreach (var b in item.OppositeLines)
                        {
                            System.Diagnostics.Debug.WriteLine($" - Line B: {b.LineB_Start} to {b.LineB_End} (L={b.LineB_LengthMm})");
                        }
                    }
                    #endregion

                    if (_viewModel.IsCreateForSingleLine) {
                        foreach (List<Curve> curves in listFramingWithSection) {
                            foreach (Curve cv in curves) {
                                _allLinesFraming = _allLinesFraming.Where(l => l != cv).ToList();
                            }
                        }
                    }

                    // THAY THẾ ProgressBar: pbWindow.Maximum = listFramingWithSection.Count;
                    double value = 0;
                    int totalBatch = listFramingWithSection.Count;

                    FamilySymbol familySymbol = null;
                    foreach (List<Curve> groupLine in listFramingWithSection) {
                        if (_trans.HasStarted()) {
                            ++value;

                            // BÁO CÁO TIẾN ĐỘ THAY CHO pbWindow.Value VÀ Title
                            _reporter.Update((int)value, totalBatch);
                            _viewModel.Percent = (value) / totalBatch * 100;

                            try {
                                double maxLength = groupLine.Max(c => c.Length);
                                Curve framingLocation = groupLine.First(c => Math.Abs(c.Length - maxLength) < 0.001);
                                Curve secondLine = groupLine.Except(new[] { framingLocation }).First();

                                XYZ base1ToBase2 = framingLocation.NormalParallelTo(secondLine);

                                #region Hàm FamilyUtils.GetFamilySymbol Framing

                                // Đi tìm tiết diện phù hợp với AutoCAD
                                foreach (var symbol in allFamilySymbol) {
                                    var bParameter = symbol.LookupParameter(_viewModel.WidthParameter);
                                    var hParameter = symbol.LookupParameter(_viewModel.HeightParameter);
                                    if (bParameter == null || hParameter == null) {
                                        TaskDialog.Show("Error",
                                            "Two parameters dimension of framing family misnamed.");
                                        return;
                                    }

                                    // đơn vị: Feet
                                    double bvalue = Convert.ToDouble(bParameter.GetValue());
                                    double hvalue = Convert.ToDouble(hParameter.GetValue());
                                    if (Math.Abs(bvalue - SonnyBIMUnitUtils.MmToFeet(rongDam)) < 0.03
                                        && Math.Abs(hvalue - SonnyBIMUnitUtils.MmToFeet(caoDam)) < 0.001) {
                                        familySymbol = symbol;
                                        break;
                                    }
                                }

                                // Nếu không tìm được tiết diện phù hợp, duplicate
                                if (familySymbol == null) {
                                    // làm tròn đến hàng đơn vị, ví dụ: 2995.5 -> 2996
                                    double sectionX = Math.Round(rongDam, 0);
                                    double sectionY = Math.Round(caoDam, 0);
                                    string name = string.Concat(sectionX, "X", sectionY);

                                    if (!name.Equals("0x0") && Math.Abs(sectionX) > 0.001 &&
                                        Math.Abs(sectionY) > 0.001) {
                                        ElementType s1 = allFamilySymbol[0].Duplicate(name);
                                        s1.LookupParameter(_viewModel.WidthParameter)
                                            ?.Set(SonnyBIMUnitUtils.MmToFeet(rongDam));
                                        s1.LookupParameter(_viewModel.HeightParameter)
                                            ?.Set(SonnyBIMUnitUtils.MmToFeet(caoDam));
                                        familySymbol = s1 as FamilySymbol;
                                    }
                                }
                                #endregion Hàm FamilyUtils.GetFamilySymbol Framing


                                if (familySymbol == null) continue;
                                if (!familySymbol.IsActive) familySymbol.Activate();

                                // Chọn dầm đầu tiên
                                if (_firstFamilySymbol == null) _firstFamilySymbol = familySymbol;
                                XYZ point1 = framingLocation.GetEndPoint(0);
                                XYZ point2 = framingLocation.GetEndPoint(1);
                                XYZ startPoint = new XYZ(point1.X, point1.Y, _viewModel.ReferenceLevel.Elevation);
                                XYZ endPoint = new XYZ(point2.X, point2.Y, _viewModel.ReferenceLevel.Elevation);
                                framingLocation = Line.CreateBound(startPoint, endPoint);

                                FamilyInstance instance = _doc.Create.NewFamilyInstance(framingLocation, familySymbol,
                                    _viewModel.ReferenceLevel, StructuralType.Beam);

                                instance.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE)
                                    .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.ZOffset));
                                instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0);
                                instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0);

                                // disallow join
                                if (_viewModel.IsDisallowJoin) {
                                    StructuralFramingUtils.DisallowJoinAtEnd(instance, 0);
                                    StructuralFramingUtils.DisallowJoinAtEnd(instance, 1);
                                    _newBeamsIds.Add(instance.Id);
                                    _newBeamsCouper.Add(instance, base1ToBase2);
                                }
                            }
                            catch {
                            }
                        }
                        else return;
                    }
                }

                #region Dựng dầm cho allLinesFraming còn lại với tiết diện đầu tiên.
                if (_viewModel.IsCreateForSingleLine) {
                    // Loại bõ những line có chiều dài bằng các bề rộng dầm sẽ dựng
                    foreach (double rd in _rongDams) {
                        _allLinesFraming = _allLinesFraming.Where(l => Math.Abs(l.Length - rd) > 0.001).ToList();
                    }

                    double valSingle = 0;
                    int totalSingle = _allLinesFraming.Count;

                    foreach (var framingLocation in _allLinesFraming) {
                        if (_trans.HasStarted()) {
                            ++valSingle;
                            _reporter.Update((int)valSingle, totalSingle);
                            _viewModel.Percent = (valSingle) / totalSingle * 100;

                            #region Code
                            XYZ point1 = framingLocation.GetEndPoint(0);
                            XYZ point2 = framingLocation.GetEndPoint(1);
                            XYZ startPoint = new XYZ(point1.X, point1.Y, _viewModel.ReferenceLevel.Elevation);
                            XYZ endPoint = new XYZ(point2.X, point2.Y, _viewModel.ReferenceLevel.Elevation);
                            Curve framingLocation2 = Line.CreateBound(startPoint, endPoint);

                            FamilyInstance framing = _doc.Create.NewFamilyInstance(framingLocation2, _firstFamilySymbol,
                                _viewModel.ReferenceLevel, StructuralType.Beam);

                            framing.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE)
                                .Set(SonnyBIMUnitUtils.MmToFeet(_viewModel.ZOffset));
                            framing.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0);
                            framing.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0);

                            int justification = framing.GetDirection().IsAlmostEqualTo(framingLocation.Direction())
                                ? 0
                                : 3;
                            framing.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(justification);

                            if (_viewModel.IsDisallowJoin) {
                                StructuralFramingUtils.DisallowJoinAtEnd(framing, 0);
                                StructuralFramingUtils.DisallowJoinAtEnd(framing, 1);
                            }
                            #endregion

                            _newBeamsIds.Add(framing.Id);
                        }
                        else return;
                    }
                }

                #endregion

                if (_trans.HasStarted()) {
                    _trans.Commit();
                    using (Transaction t2 = new Transaction(_doc, "Adjust Justification")) {
                        t2.Start();
                        foreach (var b in _newBeamsCouper) {
                            int just = b.Value.IsAlmostEqualTo(b.Key.FacingOrientation) ? 3 : 0;
                            b.Key.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(just);
                        }

                        t2.Commit();
                    }
                }
            }

            TaskDialog.Show("Inform", $"You have {_newBeamsIds.Count} beam");
        }
    }
}
