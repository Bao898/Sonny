// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Autodesk.Revit.UI;

namespace SonnyBIM
{
    public class FloorFromCADViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private SettingCreateFloor _settingToSave;
        private SettingCreateFloor _latestSetting;
        private SettingRepository<SettingCreateFloor> _settingRepository;
        public Element SelectedCadLink = null;
        public ImportInstance CadInstance = null;
        private bool _createBoundaryLineOfHatch;
        private bool _onlyCreateBoundaryLine;

        #endregion private variable

        #region public property

        [Obfuscation] public List<string> AllLayers {get; set;} = new List<string>();

        [Obfuscation] public List<FloorType> AllFloor { get; set; } = new List<FloorType>();
        [Obfuscation] public List<Level> AllLevel {get; set;} = new List<Level>();

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
        [Obfuscation] public string SelectedLayerFloor { get; set; }
        [Obfuscation] public FloorType SelectedFloor { get; set; }
        [Obfuscation] public Level SelectedLevel { get; set; }
        [Obfuscation] public double LevelOffset { get; set; }
        [Obfuscation]
        public bool OnlyCreateBoundaryLine
        {
            get => _onlyCreateBoundaryLine;
            set
            {
                _onlyCreateBoundaryLine = value;
                if (value) CreateBoundaryLineOfHatch = !value;
                OnPropertyChanged("CreateBoundaryLineOfHatch");
                OnPropertyChanged();

            }
        }
        [Obfuscation]
        public bool CreateBoundaryLineOfHatch
        {
            get => _createBoundaryLineOfHatch;
            set
            {
                _createBoundaryLineOfHatch = value;
                if (value) OnlyCreateBoundaryLine = !value;

                OnPropertyChanged("OnlyCreateBoundaryLine");
                OnPropertyChanged();
            }
        }
        [Obfuscation] public bool FromHatch { get; set; }
        [Obfuscation] public string SelectedLayerFloorHint { get; set; } = "SelectedLayerFloorHint";
        [Obfuscation] public string SelectedFloorHint { get; set; } = "SelectedFloorHint";
        [Obfuscation] public string SelectedLevelHint { get; set; } = "SelectedLevelHint";
        [Obfuscation] public string LevelOffsetHint { get; set; }
        [Obfuscation] public string LevelOffsetTooltip { get; set; }
        [Obfuscation] public string OnlyCreateBoundaryLineCap { get; set; } = "OnlyCreateBoundaryLineCap";
        [Obfuscation] public string CreateBoundaryLineOfHatchCap { get; set; } = "CreateBoundaryLineOfHatchCap";

        #endregion public property


        public FloorFromCADViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }

        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));
            _settingToSave = new SettingCreateFloor("CreateFloorFromCADSetting");
            _settingRepository = new SettingRepository<SettingCreateFloor>();
            _latestSetting = _settingRepository.GetSetting(_settingToSave.DefautSettingPath);

            try {
                SelectedCadLink = ElementSelector.PickObject(UiDoc, typeof(ImportInstance), "Select CAD Link");
            }
            catch (Exception) {
                IsCancel = true;
                return;
            }

            CadInstance = SelectedCadLink as ImportInstance;

            AllLayers = CadUtils.GetAllLayer(CadInstance, true);

            AllFloor = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Floors)
                                                        .OfClass(typeof(FloorType))
                                                        .WhereElementIsElementType()
                                                        .Cast<FloorType>().ToList();
            if (AllFloor.Count == 0) {
                MessageBox.Show("Project do have any FLoor Type!",
                    SonnyBIMConstraint.MessageBoxCaption, MessageBoxButtons.OK);
                IsCancel = true;
                return;
            }
            AllFloor.OrderBy(c=>c.Name).ToList();
            AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            LoadLatestSetting();
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage
            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng Sàn từ AutoCAD",
                "Model Floor from AutoCAD");
            SelectedLayerFloorHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Layer của nét Hatch Sàn hoặc của đường biên dạng Sàn",
                "Layer of Hatch/Line");
            SelectedLayerFloorHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Layer của nét Hatch Sàn hoặc của đường biên dạng Sàn",
                "Select Layer of Hatch/Line");
            SelectedFloorHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn kiểu Sàn",
                "Floor Type");

            SelectedLevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn tầng để tạo Sàn",
                "Level Create Floor");
            LevelOffsetHint = BindingUtils.ChangeLanguage(_languageCode,
                "Khoảng Offset(mm)",
                "Level Offset(mm)");
            OnlyCreateBoundaryLineCap = BindingUtils.ChangeLanguage(_languageCode,
                "Chỉ tạo đường biên dạng",
                "Only Create Boundary Line");
            CreateBoundaryLineOfHatchCap = BindingUtils.ChangeLanguage(_languageCode,
                "Chỉ tạo đường biên dạng của Hatch Floor",
                "Only Create Boundary Line of Hatch Floor");
            #endregion ChangeLanguage

            OnPropertyChangedAll(this);
        }

        private void LoadLatestSetting()
        {
            try {
                SelectedLayerFloor = _latestSetting.SelectedLayerFloor;
                SelectedFloor = AllFloor.FirstOrDefault(fl=>fl.Id.GetValue() == _latestSetting.SelectedFloor);
                SelectedLevel = AllLevel.FirstOrDefault(lv=>lv.Id.GetValue() == _latestSetting.SelectedLevel);
                LevelOffset = _latestSetting.LevelOffset;
                OnlyCreateBoundaryLine = _latestSetting.OnlyCreateBoundaryLine;
                FromHatch = _latestSetting.FromHatch;
                CreateBoundaryLineOfHatch = _latestSetting.CreateBoundaryLineOfHatch;
            }
            catch (Exception) {
            }
        }

        internal void SaveSetting()
        {
            try {
                _settingToSave.SelectedLayerFloor = SelectedLayerFloor;
                _settingToSave.SelectedFloor = SelectedFloor.Id.GetValue();
                _settingToSave.SelectedLevel = SelectedLevel.Id.GetValue();
                _settingToSave.LevelOffset = LevelOffset;
                _settingToSave.OnlyCreateBoundaryLine = OnlyCreateBoundaryLine;
                _settingToSave.FromHatch = FromHatch;
                _settingToSave.CreateBoundaryLineOfHatch = CreateBoundaryLineOfHatch;
                _settingRepository.SaveSetting(_settingToSave,_settingToSave.DefautSettingPath);
            }
            catch (Exception ) {
            }
        }
    }
}
