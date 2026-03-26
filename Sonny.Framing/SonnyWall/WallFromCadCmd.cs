// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Autodesk.Revit.Attributes;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class WallFromCadCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            string languageCode = LanguageData.GetLanguageSetting();

            #region

            try {
                string text = BindingUtils.ChangeLanguage(languageCode,
                    "Bạn nên lưu file Revit để đề phòng có thể gặp phải lỗi làm văng file Revit!",
                    "You should save the Revit file in case you may encounter errors making Revit out!"
                );
                using (TransactionGroup txG = new TransactionGroup(doc)) {
                    text = BindingUtils.ChangeLanguage(languageCode,
                        "Dựng Vách từ AutoCAD",
                        "Model Wall from AutoCAD");
                    txG.Start(text);

                    WallFromCADViewModel viewModel = new WallFromCADViewModel(uidoc);
                    if (viewModel.SelectedCadLink == null) { return Result.Cancelled; }

                    WallFromCadWindow window = new WallFromCadWindow(viewModel);
                    if (window.ShowDialog() == false) { return Result.Cancelled; }

                    // 1. Khởi tạo Người báo cáo tiến độ (Sử dụng ProgressView có sẵn của hệ thống)
                    var reporter = new WallProgressReporter();
                    string title = BindingUtils.ChangeLanguage(languageCode, "Dựng Vách từ AutoCAD", "Model Wall from AutoCAD");
                    reporter.Show(title);

                    try {
                        // 2. Khởi tạo Service và truyền reporter vào
                        var service = new WallServices(viewModel, reporter);

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


