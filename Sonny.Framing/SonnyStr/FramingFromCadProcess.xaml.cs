// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;
using System.Transactions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Transaction = Autodesk.Revit.DB.Transaction;
using Sonny.Framing.SonnyStr;
using Sonny.RevitExtensions.Extensions.GeometryObjects.Curves;
using MessageBox = System.Windows.Forms.MessageBox;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace SonnyBIM;

public partial class FramingFromCadProcess : Window
{
    private readonly FramingFromCADViewModel _viewModel;
    private readonly Document _doc;
    private readonly UIDocument _uidoc;
    private List<Curve> _allLinesFraming = new List<Curve>();
    private List<double> _rongDams = new List<double>();
    private FamilySymbol _firstFamilySymbol = null;
    private List<ElementId> _newBeamsIds = new List<ElementId>();
    private Dictionary<FamilyInstance, XYZ> _newBeamsCouper = new Dictionary<FamilyInstance, XYZ>();

    private Transaction _trans;

    public FramingFromCadProcess(FramingFromCADViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _doc = viewModel.Doc;
        _uidoc = viewModel.UiDoc;
        DataContext = viewModel;
        Process();
    }

    #region Copy for All xaml.cs

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try {
            // Gửi message đến hệ thống để bắt đầu di chuyển window từ non-client area (titlebar)
            SendMessage(
                new System.Runtime.InteropServices.HandleRef(this,
                    new System.Windows.Interop.WindowInteropHelper(this).Handle),
                0xA1, new IntPtr(2), IntPtr.Zero);
        }
        catch (Exception) {
            // Bỏ qua lỗi nếu có
        }
    }

    private static extern IntPtr SendMessage(System.Runtime.InteropServices.HandleRef hWnd, int msg, IntPtr wParam,
        IntPtr lParam);

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized) {
            this.WindowState = WindowState.Normal;
            MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
        }
        else {
            this.WindowState = WindowState.Maximized;
            MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter) {
            DialogResult = true;
            Close();
        }
        else if (e.Key == Key.Escape) {
            DialogResult = false;
            Close();
        }
        else if (e.Key == Key.F1) {
            BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://alphabimvn.com/en/all_plugins/");
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        if (_trans.HasStarted()) _trans.RollBack();
        MessageBoxUtils.Cancel();
    }

    #endregion Copy for All xaml.cs


    //Create a Delegate that matches the Signature of the ProgressBar's SetValue method
    [Obfuscation]
    private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

    private void Process()
    {
        List<FamilySymbol> allFamilySymbol = _viewModel.SelectedFamilyFraming.GetAllFamilySymbol();
        if (!allFamilySymbol.Any()) {
            return;
        }

        List<string> listSections = _viewModel.AllSections.Split(Convert.ToChar(";")).ToList();
        if (!listSections.Any()) {
            return;
        }

        listSections = listSections.Select(s => s.Trim()).ToList();
        _allLinesFraming = CadUtils.GetLineHaveName(_viewModel.CadInstance, _viewModel.SelectedLayer);

        // đơn vị: mm
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
                if (string.IsNullOrEmpty(section)) {
                    continue;
                }

                char[] separator = new[] { Convert.ToChar("x"), Convert.ToChar("X") };
                string[] sec = section.Split(separator);

                try {
                    rongDam = Convert.ToDouble(sec[0]);
                    caoDam = Convert.ToDouble(sec[1]);
                }
                catch (Exception) {
                    continue;
                }

                if (rongDam <= 0 || caoDam <= 0) {
                    continue;
                }

                _rongDams.Add(SonnyBIMUnitUtils.MmToFeet(rongDam));
                List<List<Curve>> listFramingWithSection = CurveUtils.GetCurvesParallelAndDistance(_allLinesFraming,
                    SonnyBIMUnitUtils.MmToFeet(rongDam));

                if (_viewModel.IsCreateForSingleLine) {
                    // Loại bõ các đường line đã thuộc về listFramingWithSection
                    foreach (List<Curve> curves in listFramingWithSection) {
                        foreach (Curve cv in curves) {
                            _allLinesFraming.Except(new[] { cv }).ToList();
                        }
                    }
                }

                pbWindow.Minimum = 0;
                pbWindow.Maximum = listFramingWithSection.Count;
                pbWindow.Value = 0;
                double value = 0;
                UpdateProgressBarDelegate updatePbDelegate = pbWindow.SetValue;

                FamilySymbol familySymbol = null;
                foreach (List<Curve> groupLine in listFramingWithSection) {
                    #region

                    if (_trans.HasStarted()) {
                        ++value;
                        try {
                            Show();
                        }
                        catch (Exception) {
                            Close();
                            _trans.RollBack();
                            MessageBoxUtils.Cancel();
                            return;
                        }

                        MainWindow.Title = string.Concat("Model Beams from AutoCAD wih section" + section,
                            "(", value, "/", pbWindow.Maximum, "/");

                        Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                            RangeBase.ValueProperty, (value + 1));

                        _viewModel.Percent = (value + 1) / pbWindow.Maximum * 100;

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
                                    MessageBox.Show(string.Concat(
                                        "Two parameters dimemsion of framing family have to name is",
                                        _viewModel.WidthParameter, _viewModel.HeightParameter));
                                    return;
                                }

                                // đơn vị: Feet
                                double bvalue = Convert.ToDouble(bParameter.GetValue());
                                double hvalue = Convert.ToDouble(hParameter.GetValue());
                                if (Math.Abs(bvalue - SonnyBIMUnitUtils.MmToFeet(rongDam)) < 0.001
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

                                if (name.Equals("0x0") || Math.Abs(sectionX) < SonnyBIMConstraint.Tolerance ||
                                    Math.Abs(sectionY) < SonnyBIMConstraint.Tolerance) {
                                    continue;
                                }

                                ElementType s1 = allFamilySymbol[0].Duplicate(name);
                                s1.LookupParameter(_viewModel.WidthParameter)?.Set(SonnyBIMUnitUtils.MmToFeet(rongDam));
                                s1.LookupParameter(_viewModel.HeightParameter)?.Set(SonnyBIMUnitUtils.MmToFeet(caoDam));
                                familySymbol = s1 as FamilySymbol;
                            }

                            #endregion Hàm FamilyUtils.GetFamilySymbol Framing

                            if (familySymbol == null) {
                                continue;
                            }

                            if (!familySymbol.IsActive) {
                                familySymbol.Activate();
                            }

                            // Chọn dầm đầu tiên
                            if (_firstFamilySymbol == null) _firstFamilySymbol = familySymbol;
                            XYZ point1 = framingLocation.GetEndPoint(0);
                            XYZ point2 = framingLocation.GetEndPoint(1);
                            XYZ startPoint = new XYZ(point1.X, point1.Y, _viewModel.ReferenceLevel.Elevation);
                            XYZ endPoint = new XYZ(point2.X, point2.Y, _viewModel.ReferenceLevel.Elevation);
                            framingLocation = Line.CreateBound(startPoint, endPoint);
                            FamilyInstance instance = _doc.Create.NewFamilyInstance(framingLocation, familySymbol,
                                _viewModel.ReferenceLevel,
                                StructuralType.Beam);

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

                        catch (Exception e) {
                        }
                    }
                    else {
                        return;
                    }

                    #endregion
                }
            }

            #region Dựng dầm cho allLinesFraming còn lại với tiết diện đầu tiên.

            if (_viewModel.IsCreateForSingleLine) {
                // Loại bõ những line có chiều dài bằng các bề rộng dầm sẽ dựng
                foreach (double rongdam in _rongDams) {
                    _allLinesFraming = _allLinesFraming.Where(l => Math.Abs(l.Length - rongdam) > 0.001).ToList();
                }

                pbWindow.Minimum = 0;
                pbWindow.Maximum = _allLinesFraming.Count;
                pbWindow.Value = 0;
                double value = 0;
                UpdateProgressBarDelegate updatePbDelegate = pbWindow.SetValue;

                foreach (var framingLocation in _allLinesFraming) {
                    #region

                    if (_trans.HasStarted()) {
                        ++value;
                        try {
                            Show();
                        }
                        catch (Exception) {
                            Close();
                            _trans.RollBack();
                            MessageBoxUtils.Cancel();
                            return;
                        }

                        MainWindow.Title = string.Concat("Model Beams from CAD for single line with section",
                            _firstFamilySymbol,
                            "(", value, "/", pbWindow.Maximum, "/");

                        Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background,
                            RangeBase.ValueProperty, (value + 1));

                        _viewModel.Percent = (value + 1) / pbWindow.Maximum * 100;

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

                        if (framing.GetDirection().IsAlmostEqualTo(framingLocation.Direction())) {
                            framing.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(0);
                        }
                        else {
                            framing.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(3);
                        }

                        // disallow join
                        if (_viewModel.IsDisallowJoin) {
                            StructuralFramingUtils.DisallowJoinAtEnd(framing, 0);
                            StructuralFramingUtils.DisallowJoinAtEnd(framing, 1);
                        }

                        #endregion

                        _newBeamsIds.Add(framing.Id);
                    }
                    else {
                        return;
                    }

                    #endregion
                }
            }

            #endregion

            //}

            if (_trans.HasStarted()) {
                _trans.Commit();
                using (Transaction trans = new Transaction(_doc)) {
                    trans.Start("x");
                    FailureHandlingOptions failOpt2 = trans.GetFailureHandlingOptions();
                    failOpt2.SetFailuresPreprocessor(new WarningDeleteWarning());
                    trans.SetFailureHandlingOptions(failOpt2);

                    foreach (var b in _newBeamsCouper) {
                        // Y_JUSTIFICATION của dầm:
                        // 1. Left: = 0
                        // 2. Center: = 1
                        // 2. Right: = 3
                        // Nếu FacingOrientation cùng hướng với base1ToBase2
                        // thì chọn Y_JUSTIFICATION = Right
                        // ngược lại, chọn Y_JUSTIFICATION = Left
                        if (b.Value.IsAlmostEqualTo(b.Key.FacingOrientation)) {
                            b.Key.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(3);
                        }
                        else {
                            b.Key.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).Set(0);
                        }
                    }

                    trans.Commit();
                }
            }
        }
        this.Close();

        TaskDialog.Show("Inform",$"You have {_newBeamsIds.Count} beam");
    }
}
