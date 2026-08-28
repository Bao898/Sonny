// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Reflection;
using System.Windows.Documents;
using Autodesk.Revit.UI;

namespace SonnyBIM
{
    public class PileCoordinateViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;

        #endregion private variable

        #region public property

        public List<Element> SelectedElements = new List<Element>();
        internal BasePoint ProjectBasePoint = null;
        [Obfuscation]
        public bool IsEntireModel { get; set; }
        [Obfuscation]
        public bool IsCurrentView { get; set; }
        [Obfuscation]
        public bool IsCurrentSelection { get; set; }
        [Obfuscation]
        public bool IsSurveyPoint { get; set; } = true;
        [Obfuscation]
        public bool IsProjectBasePoint { get; set; }
        [Obfuscation]
        public bool IsInternalOrigin { get; set; }

        [Obfuscation]
        public LanguageData SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value ?? AllLanguage[0];
                _languageCode = _selectedLanguage.Code;

                ChangeLanguage();
                //LanguageHelper.ChangeLanguage(MainWindow, languageCode, "OnlineLicense", "AlphaBIM");

                SonnySetting.SaveSonnySetting(_languageCode);
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public string CoordinateBaseCap { get; set; }
        [Obfuscation]
        public string ScopePilesCap { get; set; }
        [Obfuscation]
        public string EntireModelCap { get; set; }
        [Obfuscation]
        public string CurrentViewCap { get; set; }
        [Obfuscation]
        public string CurrentSelectionCap { get; set; }
        [Obfuscation]
        public string SurveyPointCap { get; set; }
        [Obfuscation]
        public string ProjectBasePointCap { get; set; }
        [Obfuscation]
        public string InternalOriginCap { get; set; }


        #endregion public property
        public PileCoordinateViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }

        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));

            List<ElementFilter> filters = new List<ElementFilter>();
            filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation));
            filters.Add(new  ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns));

            LogicalOrFilter orFilter = new LogicalOrFilter(filters);
            Element firstOrDefault = new FilteredElementCollector(Doc).WherePasses(orFilter)
                .WhereElementIsNotElementType().FirstOrDefault();

            if (firstOrDefault == null) {
                IsCancel = true;
                MessageBox.Show("The project does not have any Piles Foundation or Structural Column!", SonnyBIMConstraint.MessageBoxCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Tạo Shared parameter
            Parameter p = firstOrDefault.LookupParameter(PileCoordinateParameters.XCoorPBP_Parameter);

            if (p == null) {
                using (Transaction trans = new Transaction(Doc)) {
                    trans.Start("Create Shared Parameter");
                    WarningDeleteWarning deleteWarning = new WarningDeleteWarning();
                    FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
                    failOpt.SetFailuresPreprocessor(deleteWarning);
                    trans.SetFailureHandlingOptions(failOpt);

                    List<Category> cat = new List<Category>()
                    {
                        Category.GetCategory(Doc, BuiltInCategory.OST_StructuralFoundation),
                        Category.GetCategory(Doc, BuiltInCategory.OST_StructuralColumns),
                    };

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.XCoorSP_Parameter,
                        PileCoordinateParameters.XCoorSP_ParameterType,
                        PileCoordinateParameters.XCoorSP_UnderGroup,
                        PileCoordinateParameters.XCoorSP_Description, cat);

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.YCoorSP_Parameter,
                        PileCoordinateParameters.YCoorSP_ParameterType,
                        PileCoordinateParameters.YCoorSP_UnderGroup,
                        PileCoordinateParameters.YCoorSP_Description, cat);

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.XCoorPBP_Parameter,
                        PileCoordinateParameters.XCoorPBP_ParameterType,
                        PileCoordinateParameters.XCoorPBP_UnderGroup,
                        PileCoordinateParameters.XCoorPBP_Description, cat);

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.YCoorPBP_Parameter,
                        PileCoordinateParameters.YCoorPBP_ParameterType,
                        PileCoordinateParameters.YCoorPBP_UnderGroup,
                        PileCoordinateParameters.YCoorPBP_Description, cat);

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.XCoorInternalOrigin_Parameter,
                        PileCoordinateParameters.XCoorInternalOrigin_ParameterType,
                        PileCoordinateParameters.XCoorInternalOrigin_UnderGroup,
                        PileCoordinateParameters.XCoorInternalOrigin_Description, cat);

                    ParameterUtils.CreateSharedParameter(Doc, SonnyBIMConstraint.SharedParamsGroup_STR,
                        PileCoordinateParameters.YCoorInternalOrigin_Parameter,
                        PileCoordinateParameters.YCoorInternalOrigin_ParameterType,
                        PileCoordinateParameters.YCoorInternalOrigin_UnderGroup,
                        PileCoordinateParameters.YCoorInternalOrigin_Description, cat);

                    trans.Commit();
                }
            }

            if (UiDoc.Selection.GetElementIds().Any()) IsCurrentSelection = true;
            ProjectBasePoint = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_ProjectBasePoint).First() as BasePoint;
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng Cọc từ AutoCAD",
                "Model Pile from AutoCAD");

            CoordinateBaseCap = BindingUtils.ChangeLanguage(_languageCode,
                "Hệ tọa độ",
                "Base Coordinate");
            ScopePilesCap = BindingUtils.ChangeLanguage(_languageCode,
                "Phạm vi tính toán",
                "Scope Pile/Column");
            EntireModelCap = BindingUtils.ChangeLanguage(_languageCode,
                "Toàn Bộ Dự Án",
                "Entire Project");
            CurrentViewCap = BindingUtils.ChangeLanguage(_languageCode,
                "View Hiện Tại",
                "Current View");
            CurrentSelectionCap = BindingUtils.ChangeLanguage(_languageCode,
                "Các Đối Tượng Được Chọn",
                "Current Selection");

            SurveyPointCap = BindingUtils.ChangeLanguage(_languageCode,
                "Survey Point",
                "Survey Point");
            ProjectBasePointCap = BindingUtils.ChangeLanguage(_languageCode,
                "Project Base Point",
                "Project Base Point");
            InternalOriginCap = BindingUtils.ChangeLanguage(_languageCode,
                "Internal Origin",
                "Internal Origin");

            #endregion ChangeLanguage

            OnPropertyChangedAll(this);
        }

        private void LoadLatestSetting()
        {

        }

        internal void SaveSetting()
        {

        }
    }
}
