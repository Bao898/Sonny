// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Sonny.Application.Presentation.Implements;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Sonny.Application.Domain.Services;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class PileCoordinateCmd : IExternalCommand
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
                string text = BindingUtils.ChangeLanguage(languageCode,
                    "Bạn nên lưu file Revit để đề phòng có thể gặp phải lỗi làm văng file Revit!",
                    "You should save the Revit file in case you may encounter errors making Revit out!"
                );
                using (TransactionGroup txG = new TransactionGroup(doc)) {
                    text = BindingUtils.ChangeLanguage(languageCode,
                        "Tính toán tọa độ Cọc",
                        "Pile Coordinates");
                    txG.Start(text);

                    PileCoordinateViewModel viewModel = new PileCoordinateViewModel(uidoc);
                    if (viewModel.IsCancel) { return Result.Cancelled; }

                    PileCoordinateWindow window = new PileCoordinateWindow(viewModel);
                    if (window.ShowDialog() == false) { return Result.Cancelled; }

                    // 1. Khởi tạo Người báo cáo tiến độ (Sử dụng ProgressView có sẵn của hệ thống)
                    var reporter = new PileCoordinateProgressReporter();
                    string title = BindingUtils.ChangeLanguage(languageCode, "Tính toán tọa độ Cọc",
                        "Pile Coordinates");
                    reporter.Show(title);

                    new PileCoordinateServices(viewModel, reporter);

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


