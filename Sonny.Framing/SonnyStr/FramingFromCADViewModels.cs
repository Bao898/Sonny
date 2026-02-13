// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Autodesk.Revit.UI;
using Sonny.Framing.SonnyStr;

namespace SonnyBIM
{
    public class FramingFromCADViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private Family _selectedFamilyFraming;
        private List<string> _allFramingTypeParameter = new List<string>();

        private SettingCreateFraming _settingToSave;
        private SettingCreateFraming _latestSetting;
        private SettingRepository<SettingCreateFraming> _settingRepository;
        public Element SelectedCadLink = null;
        public ImportInstance CadInstance = null;
        private string _heightParameter;
        private string _widthParameter;

        #endregion private variable

        #region public property
        [Obfuscation]
        public List<string> AllLayers {get; set;} = new List<string>();

        [Obfuscation]
        public double ZOffset {get; set;}
        [Obfuscation]
        public string AllSections {get; set;}
        [Obfuscation]
        public bool IsCreateForSingleLine {get; set;}

        [Obfuscation]
        public List<Level> AllLevel {get; set;} = new List<Level>();
        [Obfuscation]
        public Level ReferenceLevel {get; set;}

        [Obfuscation]
        public string SelectedLayer {get; set;}

        [Obfuscation]
        public List<Family> AllFamiliesFraming {get; set;} = new List<Family>();

        [Obfuscation]
        public Family SelectedFamilyFraming
        {
            get => _selectedFamilyFraming;
            set
            {
                _selectedFamilyFraming = value;
                FamilySymbol first = value.GetAllFamilySymbol().First();

                AllFramingTypeParameter = ParameterUtils.GetAllTypeParameters(first);
                AllFramingTypeParameter = AllFramingTypeParameter.Where(p => !p.Contains("Assembly"))
                    .Where(p => !p.Contains("OmniClass"))
                    .Where(p => !p.Contains("Material"))
                    .Where(p => !p.Contains("Category"))
                    .Where(p => !p.Contains("Type")).ToList();
                HeightParameter = AllFramingTypeParameter[0];
                WidthParameter = AllFramingTypeParameter[0];
            }
        }

        [Obfuscation]
        public List<string> AllFramingTypeParameter {
            get => _allFramingTypeParameter;
            set
            {
                _allFramingTypeParameter = value;
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public LanguageData SelectedLanguage {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value ?? AllLanguage[0];
                _languageCode = _selectedLanguage.Code;

                ChangeLanguage();

                SonnySetting.SaveSonnySetting(_languageCode);
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public string HeightParameter {
            get => _heightParameter;
            set
            {
                _heightParameter = value;
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public string WidthParameter {
            get => _widthParameter;
            set
            {
                _widthParameter = value;
                OnPropertyChanged();
            }
        }
        [Obfuscation]
        public string SelectedLayerHint {get; set;}

        [Obfuscation]
        public string SelectedFamilyFramingHint {get; set;}
        [Obfuscation]
        public string WidthParameterHint { get; set; }
        [Obfuscation]
        public string HeightParameterHint { get; set; }
        [Obfuscation]
        public string ReferenceLevelHint { get; set; }
        [Obfuscation]
        public string ZOffsetHint { get; set; }
        [Obfuscation]
        public string ZOffsetToolTip { get; set; }
        [Obfuscation]
        public string AllSectionsHint { get; set; }
        [Obfuscation]
        public string AllSectionsToolTip { get; set; }
        [Obfuscation]
        public string IsCreateForSingleLineCap { get; set; }
        [Obfuscation]
        public string IsCreateForSingleLineToolTip { get; set; }
        [Obfuscation]
        public string IsDisallowJoinCap { get; set; }
        [Obfuscation]
        public string IsDisallowJoinToolTip { get; set; }
        [Obfuscation]
        public bool IsDisallowJoin { get; set; }

        public UIDocument? UiDoc { get; set; }

        #endregion public property

        public FramingFromCADViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }
        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));
            _settingToSave = new SettingCreateFraming("CreateFramingFromCADSetting");
            _settingRepository = new SettingRepository<SettingCreateFraming>();
            _latestSetting = _settingRepository.GetSetting(_settingToSave.DefautSettingPath);

            List<Type> typesFilter = new List<Type>() { typeof(ImportInstance), };

            try {
                SelectedCadLink =
                    ElementSelector.PickObject(UiDoc, new ClassSelectionFilter(typesFilter), "Select CAD Link");
            }
            catch (Exception) {
                return;
            }

            CadInstance = SelectedCadLink as ImportInstance;

            AllLayers = CadUtils.GetAllLayer(CadInstance);

            SelectedLayer = AllLayers.FirstOrDefault(x=>x.Contains("Beam"));

            if (string.IsNullOrEmpty(SelectedLayer))
            {
                SelectedLayer = AllLayers[0];
            }

            Category category = Category.GetCategory(Doc, BuiltInCategory.OST_StructuralFraming);
            AllFamiliesFraming = new FilteredElementCollector(Doc).OfClass(typeof(Family)).Cast<Family>().
                Where(x => x.FamilyCategory.Id.Equals(category.Id)).ToList();

            if (AllFamiliesFraming.Count == 0) {
                string text = BindingUtils.ChangeLanguage(_languageCode,
                    "Không có Dầm trong dự án. Vui lòng load Family Dầm vào dự án và chạy lại lệnh.",
                    "None Framing in project. Please load family Framing into project before run this Add-in!");
                MessageBox.Show(text, SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK);
                return;
            }

            AllFamiliesFraming = AllFamiliesFraming.Where(x => x.GetFamilySymbolIds().Any()).OrderBy(c =>c.Name).ToList();
            SelectedFamilyFraming = AllFamiliesFraming[0];

            FamilySymbol first = SelectedFamilyFraming.GetAllFamilySymbol().First();

            AllFramingTypeParameter = ParameterUtils.GetAllTypeParameters(first);
            AllFramingTypeParameter = AllFramingTypeParameter.Where(p => !p.Contains("Assembly"))
                .Where(p => !p.Contains("OmniClass"))
                .Where(p => !p.Contains("Material"))
                .Where(p => !p.Contains("Category"))
                .Where(p => !p.Contains("Type")).ToList();

            WidthParameter = string.IsNullOrEmpty(AllFramingTypeParameter.FirstOrDefault(p => p.Equals("b")))
                ? AllFramingTypeParameter[0] : AllFramingTypeParameter.FirstOrDefault(p => p.Equals("b"));
            HeightParameter = string.IsNullOrEmpty(AllFramingTypeParameter.FirstOrDefault(p => p.Equals("h")))
                ? AllFramingTypeParameter[0] : AllFramingTypeParameter.FirstOrDefault(p => p.Equals("h"));

            AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            AllLevel = AllLevel.OrderBy(level => level.Elevation).ToList();
            ReferenceLevel = AllLevel.FirstOrDefault(l => l.Id.Equals(Doc.ActiveView.GenLevel?.Id));

            LoadLatestSetting();
        }

        public void LoadLatestSetting()
        {
            try {
                SelectedLayer = _latestSetting.SelectedLayer;
                try {
                    SelectedFamilyFraming = AllFamiliesFraming.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedFamilyFraming));
                }
                catch (Exception ) {
                }

                HeightParameter = _latestSetting.HeightParameter;
                WidthParameter = _latestSetting.WidthParameter;

                ReferenceLevel = AllLevel.First(x => x.Id.GetValue().Equals(_latestSetting.ReferenceLevel));
                ZOffset = _latestSetting.ZOffset;
                AllSections = _latestSetting.AllSections;
                IsCreateForSingleLine = _latestSetting.IsCreateForSingleLine;
            }
            catch (Exception ) {
            }
        }

        internal void SaveSetting()
        {
            try {
                _settingToSave.SelectedLayer = SelectedLayer;
                try {
                    _settingToSave.SelectedFamilyFraming = SelectedFamilyFraming.Id.GetValue();
                }
                catch (Exception ) {
                }

                _settingToSave.HeightParameter = HeightParameter;
                _settingToSave.WidthParameter = WidthParameter;

                try {
                    _settingToSave.ReferenceLevel = ReferenceLevel.Id.GetValue();
                }
                catch (Exception ) {
                }

                _settingToSave.ZOffset = ZOffset;
                _settingToSave.AllSections = AllSections;
                _settingToSave.IsCreateForSingleLine = IsCreateForSingleLine;
                _settingRepository.SaveSetting(_settingToSave,_settingToSave.DefautSettingPath);
            }
            catch (Exception ) {
            }
        }
        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Tự động dựng Dầm từ bản vẽ AutoCAD",
                "Model Framing from AutoCAD");

            SelectedLayerHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Layer của nét Dầm",
                "Choose Layer of Beam");

            SelectedFamilyFramingHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Family Dầm",
                "Choose Beam Family");


            WidthParameterHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Parameter Chiều Rộng Dầm",
                "Choose Width Parameter");


            HeightParameterHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Parameter Chiều Cao Dầm",
                "Choose Height Parameter");


            ReferenceLevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Level Đặt Dầm",
                "Choose Reference Level");


            ZOffsetHint = BindingUtils.ChangeLanguage(_languageCode,
                "Đặt Giá Trị z Offset",
                "Set z Offset Value");

            ZOffsetToolTip = BindingUtils.ChangeLanguage(_languageCode,
                "Đơn vị là millimeter",
                "The unit is millimeters");


            ZOffsetToolTip = BindingUtils.ChangeLanguage(_languageCode,
                "Đơn vị là millimeter",
                "The unit is millimeters");

            AllSectionsHint = BindingUtils.ChangeLanguage(_languageCode,
                "Các tiết diện của Dầm, ví dụ: 200x300; 400x600...",
                "All sections of Beam, example: 200x300; 400x600...");


            AllSectionsToolTip = BindingUtils.ChangeLanguage(_languageCode,
                "Nhập vào các tiết diện của Dầm sẽ được tạo, đơn vị là millimeter, cách nhau bởi dấu ;" + "\n" +
                "Ví dụ: 200x300; 300x700; 400x600",
                "The sections of Beams(with the unit is millimeter) will be created, separated by the ; " + "\n" +
                "For example: 200x300; 300x700; 400x600");

            IsCreateForSingleLineCap = BindingUtils.ChangeLanguage(_languageCode,
                "Tạo Dầm cho Từng Đường Thẳng?",
                "Create for Single Line?");

            IsCreateForSingleLineToolTip = BindingUtils.ChangeLanguage(_languageCode,
                "Sử dụng tiết diện đầu tiên trong danh sách tất cả tiết diện Dầm để tạo Dầm cho từng đường thẳng.",
                "Use the first section in All Sections to create Beam for each Single Line.");


            IsDisallowJoinCap = BindingUtils.ChangeLanguage(_languageCode,
                "Disallow Join",
                "Disallow Join");

            IsDisallowJoinToolTip = BindingUtils.ChangeLanguage(_languageCode,
                "Các dầm mới được tạo ra sẽ bị Disallow Join ở 2 đầu dầm",
                "Newly created framings will be Disallow Join at both ends");

            #endregion
        }
    }
}
