// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Sonny.Framing.SonnyStr;

namespace SonnyBIM
{
    [Transaction(TransactionMode.Manual)]
    public class FramingFromCadCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            string languageCode = LanguageData.GetLanguageSetting();

            string addinName = Q8fdaa690396dbff69bab7eec947f57fc.ModelFromAcad.Item1;

            #region

            try {
                string text = BindingUtils.ChangeLanguage(languageCode,
                    "Bạn nên lưu file Revit để đề phòng có thể gặp phải lỗi làm văng file Revit!",
                    "You should save the Revit file in case you may encounter errors making Revit out!"
                );
                using (TransactionGroup txG = new TransactionGroup(doc)) {
                    text = BindingUtils.ChangeLanguage(languageCode,
                        "Dựng Dầm từ AutoCAD",
                        "Model Framing from AutoCAD");
                    txG.Start(text);

                    FramingFromCADViewModel viewModel = new FramingFromCADViewModel(uidoc);
                    if (viewModel.SelectedCadLink == null) { return Result.Cancelled; }

                    FramingFromCadWindow window = new FramingFromCadWindow(viewModel);
                    if (window.ShowDialog() == false) { return Result.Cancelled; }
                    new FramingFromCadProcess(viewModel);

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


