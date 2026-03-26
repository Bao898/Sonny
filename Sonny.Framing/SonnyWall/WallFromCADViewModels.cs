// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Autodesk.Revit.UI;
using System.Windows.Forms;


namespace SonnyBIM
{
    public class WallFromCADViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private SettingCreateWall _settingToSave;
        private SettingCreateWall _latestSetting;
        private SettingRepository<SettingCreateWall> _settingRepository;
        public Element SelectedCadLink = null;
        public ImportInstance CadInstance = null;
        private bool _forThicknessType1;
        private bool _forThicknessType2;
        private bool _forThicknessType3;

        #endregion private variable

        #region public property
        [Obfuscation]
        public List<string> AllLayers {get; set;} = new List<string>();
        [Obfuscation]
        public string SelectedLayer { get; set; }
        [Obfuscation]
        public List<WallType> AllWallType { get; set; } = new List<WallType> ();
        [Obfuscation]
        public List<Level> AllLevel {get; set;} = new List<Level>();
        [Obfuscation]
        public WallType SelectedWallType1 { get; set; }
        [Obfuscation]
        public WallType SelectedWallType2 { get; set; }
        [Obfuscation]
        public WallType SelectedWallType3 { get; set; }
        [Obfuscation]
        public bool ForThicknessType1 {
            get => _forThicknessType1;
            set
            {
                _forThicknessType1 = IsEnabledThicknessType1 = value;
                OnPropertyChanged("IsEnabledThicknessType1");
            }
        }
        [Obfuscation]
        public string AllThicknessType1 { get; set; }
        [Obfuscation]
        public bool IsEnabledThicknessType1 { get; set; }

        [Obfuscation]
        public bool ForThicknessType2 {
            get => _forThicknessType2;
            set
            {
                _forThicknessType1 = IsEnabledThicknessType2 = value;
                OnPropertyChanged("IsEnabledThicknessType1");
            }
        }
        [Obfuscation]
        public string AllThicknessType2 { get; set; }
        [Obfuscation]
        public bool IsEnabledThicknessType2 { get; set; }

        [Obfuscation]
        public bool ForThicknessType3 {
            get => _forThicknessType3;
            set
            {
                _forThicknessType1 = IsEnabledThicknessType3 = value;
                OnPropertyChanged("IsEnabledThicknessType1");
            }
        }
        [Obfuscation]
        public string AllThicknessType3 { get; set; }
        [Obfuscation]
        public bool IsEnabledThicknessType3 { get; set; }

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
        public Level BaseLevel { get; set; }
        [Obfuscation]
        public Level TopLevel { get; set; }
        [Obfuscation]
        public double BaseOffset { get; set; }
        [Obfuscation]
        public double TopOffset { get; set; }
        [Obfuscation]
        public bool IsCreateWallStructural { get; set; }
        [Obfuscation]
        public string SelectedWallType1Hint {get; set;}
        [Obfuscation]
        public string SelectedWallType2Hint {get; set;}
        [Obfuscation]
        public string SelectedWallType3Hint {get; set;}
        [Obfuscation]
        public string BaseLevelHint {get; set;}
        [Obfuscation]
        public string TopLevelHint {get; set;}
        [Obfuscation]
        public string BaseOffsetHint {get; set;}
        [Obfuscation]
        public string TopOffsetHint {get; set;}
        [Obfuscation]
        public string StructuralWallCap {get; set;}

        #endregion public property


        public WallFromCADViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }
        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));

            _settingToSave = new SettingCreateWall("CreateWallFromCADSetting");
            _settingRepository = new SettingRepository<SettingCreateWall>();
            _latestSetting = _settingRepository.GetSetting(_settingToSave.DefautSettingPath);

            List<Type> typesFilter = new List<Type>()
            {
                typeof(ImportInstance),
            };
            try {
                SelectedCadLink = ElementSelector.PickObject(UiDoc, new ClassSelectionFilter(typesFilter), "Select CAD Link");
            }
            catch (Exception ) {
                return;
            }

            CadInstance = SelectedCadLink as ImportInstance;
            AllLayers = CadUtils.GetAllLayer(CadInstance, true);
            SelectedLayer = AllLayers[0];

            AllWallType = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .Cast<WallType>().ToList();
            if (AllWallType.Count == 0) {
                MessageBox.Show("Project do not have any Wall Type!",
                    SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK);
                return;
            }

             AllWallType = AllWallType.OrderBy(c => c.Name).ToList();
             AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
             AllLevel = AllLevel.OrderBy(l => l.Elevation).ToList();
             LoadLatestSetting();
        }

        internal void LoadLatestSetting()
        {
            try {
                SelectedLayer = _latestSetting.SelectedLayer;
                try {
                    SelectedWallType1 = AllWallType.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedWallType1));
                }
                catch (Exception ) {

                }
                try {
                    SelectedWallType2 = AllWallType.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedWallType2));
                }
                catch (Exception ) {

                }
                try {
                    SelectedWallType3 = AllWallType.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedWallType3));
                }
                catch (Exception ) {

                }

                ForThicknessType1 = _latestSetting.ForThicknessType1;
                AllThicknessType1 = _latestSetting.AllThicknessType1;
                ForThicknessType2 = _latestSetting.ForThicknessType2;
                AllThicknessType2 = _latestSetting.AllThicknessType2;
                ForThicknessType3 = _latestSetting.ForThicknessType3;
                AllThicknessType3 = _latestSetting.AllThicknessType3;

                BaseLevel = AllLevel.First(l => l.Id.GetValue().Equals(_latestSetting.BaseLevel));
                TopLevel = AllLevel.First(l => l.Id.GetValue().Equals(_latestSetting.TopLevel));
                IsCreateWallStructural = _latestSetting.IsCreateWallStructural;
            }
            catch (Exception) {
            }
        }

        internal void SaveSetting()
        {
            try {
                _settingToSave.SelectedLayer = SelectedLayer;
                try {
                    _settingToSave.SelectedWallType1 = (int)SelectedWallType1.Id.GetValue();
                }
                catch (Exception) {

                }
                try {
                    _settingToSave.SelectedWallType2 = (int)SelectedWallType2.Id.GetValue();
                }
                catch (Exception) {

                }
                try {
                    _settingToSave.SelectedWallType3 = (int)SelectedWallType3.Id.GetValue();
                }
                catch (Exception) {

                }
                _settingToSave.ForThicknessType1 = ForThicknessType1;
                _settingToSave.AllThicknessType1 = AllThicknessType1;
                _settingToSave.ForThicknessType2 = ForThicknessType2;
                _settingToSave.AllThicknessType2 = AllThicknessType2;
                _settingToSave.ForThicknessType3 = ForThicknessType3;
                _settingToSave.AllThicknessType3 = AllThicknessType3;

                try {
                    _settingToSave.BaseLevel = (int)BaseLevel.Id.GetValue();
                }
                catch (Exception ) {

                }
                try {
                    _settingToSave.TopLevel = (int)TopLevel.Id.GetValue();
                }
                catch (Exception ) {

                }
                _settingToSave.BaseOffset = BaseOffset;
                _settingToSave.TopOffset = TopOffset;
                _settingToSave.IsCreateWallStructural = IsCreateWallStructural;
                _settingRepository.SaveSetting(_settingToSave, _settingToSave.DefautSettingPath);

            }
            catch (Exception) {

            }
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Tự động dựng Vách từ bản vẽ AutoCAD",
                "Model Wall from AutoCAD");

            SelectedWallType1Hint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn kiểu Vách",
                "Choose Wall Type");

            SelectedWallType2Hint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn kiểu Vách",
                "Choose Wall Type");

            SelectedWallType3Hint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn kiểu Vách",
                "Choose Wall Type");

            BaseLevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn tầng đáy",
                "Choose Base Level");

            TopLevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn tầng đỉnh",
                "Choose Top Level");

            BaseOffsetHint = BindingUtils.ChangeLanguage(_languageCode,
                "Offset tại đáy Vách",
                "Set Base Offset");

            TopOffsetHint = BindingUtils.ChangeLanguage(_languageCode,
                "Offset tại đỉnh Vách",
                "Set Top Offset");

            StructuralWallCap = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng Vách kết cấu",
                "Structural Wall");
            #endregion

            OnPropertyChangedAll(this);
        }
    }

}

