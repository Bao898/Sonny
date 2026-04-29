// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Windows.Documents;
using Autodesk.Revit.UI;

namespace SonnyBIM
{
    public class FoundationFromCADViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private SettingCreateFoundation _settingToSave;
        private SettingCreateFoundation _latestSetting;
        private SettingRepository<SettingCreateFoundation> _settingRepository;
        public Element SelectedCadLink = null;
        public ImportInstance CadInstance = null;

        #endregion private variable

        #region public property

        [Obfuscation] public List<string> AllLayers {get; set;} = new List<string>();
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public FloorType SelectedFloor { get; set; }
        [Obfuscation] public Level SelectedLevel { get; set; }
        [Obfuscation] public double OffsetFromElevation { get; set; }
        public double OffsetFromElevationToRevitUnit => OffsetFromElevation.MmToFeet();
        [Obfuscation] public List<FloorType> AllFloor { get; set; } = new List<FloorType>();
        [Obfuscation] public List<Level> AllLevel { get; set; } = new List<Level>();
        [Obfuscation] public string SelectedLayerHint { get; set; }
        [Obfuscation] public string FoundationTypeHint { get; set; }
        [Obfuscation] public string LevelHint { get; set; }
        [Obfuscation] public string OffsetHint { get; set; }

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

        #endregion public property


        public FoundationFromCADViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }

        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));
            _settingToSave = new SettingCreateFoundation("CreateFoundationFromCADSetting");
            _settingRepository = new SettingRepository<SettingCreateFoundation>();
            _latestSetting = _settingRepository.GetSetting(_settingToSave.DefautSettingPath);

            List<Type> typesFilter = new List<Type>()
            {
                typeof(ImportInstance),
            };
            try {
                SelectedCadLink = ElementSelector.PickObject(UiDoc, new ClassSelectionFilter(typesFilter),"Select a AutoCAD Link");
            }
            catch (Exception) {
                return;
            }

            CadInstance = SelectedCadLink as ImportInstance;
            AllLayers = CadUtils.GetAllLayer(CadInstance);
            SelectedLayer = AllLayers[0];

            AllFloor = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .OfClass(typeof(FloorType))
                .WhereElementIsElementType().Cast<FloorType>().ToList();
            AllFloor = AllFloor.OrderBy(floor => floor.Name).ToList();
            SelectedFloor = AllFloor[0];

            AllLevel = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            SelectedLevel = AllLevel[0];

            LoadLatestSetting();
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng Sàn từ AutoCAD",
                "Model Foundation from AutoCAD");
            SelectedLayerHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Layer Đường Biên Dạng của Đài Móng",
                "Boundary Layer Slab Foundation");
            FoundationTypeHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Loại Đài Móng",
                "Foundation Type");
            LevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Level Đặt Đài Móng",
                "Level Slab Foundation");
            OffsetHint= BindingUtils.ChangeLanguage(_languageCode,
                "Đặt Offset",
                "Height Offset From Level");

            #endregion ChangeLanguage

            OnPropertyChangedAll(this);
        }

        private void LoadLatestSetting()
        {
            try {
                SelectedLayer = _latestSetting.SelectedLayer;
                try {
                    SelectedFloor = AllFloor.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedFloor));
                }
                catch (Exception ) { }

                SelectedLevel = AllLevel.First(l => l.Id.GetValue().Equals(_latestSetting.SelectedLevel));
            }
            catch (Exception) { }
        }

        internal void SaveSetting()
        {
            try {
                _settingToSave.SelectedLayer = SelectedLayer;
                try {
                    _settingToSave.SelectedFloor = SelectedFloor.Id.GetValue();
                }
                catch (Exception) { }

                try {
                    _settingToSave.SelectedLevel = SelectedLevel.Id.GetValue();
                }
                catch (Exception ) { }
                _settingRepository.SaveSetting(_settingToSave,_settingToSave.DefautSettingPath);
            }
            catch (Exception ) { }
        }
    }
}
