// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Sonny.Application.Presentation.Implements;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class PileFromCadCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            string languageCode = LanguageData.GetLanguageSetting();

            //string addinName = Q8fdaa690396dbff69bab7eec947f57fc.ModelFromAcad.Item1;

            #region

            try {

                using (TransactionGroup txG = new TransactionGroup(doc)) {
                    string text = BindingUtils.ChangeLanguage(languageCode,
                        "Dựng Móng từ AutoCAD",
                        "Model Pile from AutoCAD");
                    txG.Start(text);

                    PileFromCADViewModel viewModel = new PileFromCADViewModel(uidoc);
                    if (viewModel.SelectedCadLink == null) { return Result.Cancelled; }

                    PileFromCadWindow window = new PileFromCadWindow(viewModel);
                    if (window.ShowDialog() == false) { return Result.Cancelled; }

                    // 1. Khởi tạo Người báo cáo tiến độ (Sử dụng ProgressView có sẵn của hệ thống)
                    var reporter = new PileProgressReporter();
                    string title = BindingUtils.ChangeLanguage(languageCode, "Dựng Móng từ AutoCAD", "Model Pile from AutoCAD");
                    reporter.Show(title);

                    try {
                        // 2. Khởi tạo Service và truyền reporter vào
                        var service = new PileServices(viewModel, reporter);

                        // 3. Thực hiện thuật toán
                        service.Execute();
                    }
                    catch (Exception ex) {
                        // Hiển thị lỗi nếu có vấn đề trong quá trình chạy
                        TaskDialog.Show("Error", ex.Message);
                    }
                    finally {
                        // 4. Luôn đảm bảo đóng cửa sổ Progress khi kết thúc (thành công hoặc thất bại)
                        reporter.Close();
                    }
                    txG.Assimilate();
                }
                return Result.Succeeded;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }

            return Result.Cancelled;

            #endregion
        }

    }
}


