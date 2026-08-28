// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Sonny.Application.Presentation.Implements;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Sonny.Application.Domain.Services;
using System.Reflection;
using Path = System.IO.Path;
using View = Autodesk.Revit.DB.View;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class AutoJoinCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            string languageCode = LanguageData.GetLanguageSetting();

            string dllFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            AssemblyLoader.LoadAssemblies(dllFolder, "MaterialDesignThemes.Wpf.dll");
            AssemblyLoader.LoadAssemblies(dllFolder, "MaterialDesignColors.dll");
            AssemblyLoader.LoadAssemblies(dllFolder, "AlphaBIMResources.dll");

            if (true) {
                if (doc.IsFamilyDocument) {
                    string text = BindingUtils.ChangeLanguage(languageCode,
                        "AutoJoin add-in không chạy được trên môi trường Family.",
                        "AutoJoin add-in can't run on Family environment.");
                    MessageBox.Show(text, SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                View currentView = doc.ActiveView;
                if (currentView.ViewType == ViewType.Schedule || currentView.ViewType == ViewType.ColumnSchedule
                    || currentView.ViewType == ViewType.DrawingSheet) {
                    string text = BindingUtils.ChangeLanguage(languageCode,
                        "Auto Join add-in không chạy được trên loại view này.",
                        "Auto Join add-in can't run on this View type.");
                    MessageBox.Show(text, SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                try {
                    string text = BindingUtils.ChangeLanguage(languageCode,
                        "Bạn nên lưu file Revit để đề phòng có thể gặp phải lỗi làm văng file Revit!",
                        "You should save the Revit file in case you may encounter errors making Revit out!");

                    using (TransactionGroup txG = new TransactionGroup(doc)) {
                        txG.Start("Sony Bim | Auto Join");

                        AutoJoinViewModel viewModel = new AutoJoinViewModel(uidoc);
                        if (viewModel.IsCancel) {
                            return Result.Cancelled;}

                        AutoJoinWindow window = new AutoJoinWindow(viewModel);
                        if (window.ShowDialog() == false) { return Result.Cancelled; }

                        // 1. Khởi tạo Người báo cáo tiến độ (Sử dụng ProgressView có sẵn của hệ thống)
                        var reporter = new AutoJoinProgressReporter();
                        string title = BindingUtils.ChangeLanguage(languageCode, "Auto Join", "Auto Join");
                        reporter.Show(title);

                        try {
                            // 2. Khởi tạo Service và truyền reporter vào
                            var service = new AutoJoinServices(viewModel, reporter);
                        }
                        catch (Exception ex) {
                            // Hiển thị lỗi nếu có vấn đề trong quá trình chạy
                            TaskDialog.Show("Error", ex.Message);
                        }

                        txG.Assimilate();
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return Result.Cancelled;

        }
}
}


