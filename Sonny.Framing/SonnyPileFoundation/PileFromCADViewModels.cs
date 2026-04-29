// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Reflection;
using System.Windows.Documents;
using Autodesk.Revit.UI;

namespace SonnyBIM
{
    public class PileFromCADViewModel : ViewModelBaseNew
    {
        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private SettingCreatePile _settingToSave;
        private SettingCreatePile _latestSetting;
        private SettingRepository<SettingCreatePile> _settingRepository;
        public Element SelectedCadLink = null;
        public ImportInstance CadInstance = null;

        private bool _isModelByTwoLine = true;
        private bool _isModelByCircle;

        #endregion private variable

        #region public property

        [Obfuscation] public List<string> AllLayers { get; set; } = new List<string>();
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public List<FamilySymbol> AllPile { get; set; } = new List<FamilySymbol>();
        [Obfuscation] public FamilySymbol SelectedPile { get; set; }
        [Obfuscation] public List<Family> AllFamilies { get; set; } = new List<Family>();
        [Obfuscation] public Family SelectedFamily { get; set; }
        [Obfuscation] public List<string> AllPileTypeParameter { get; set; } = new List<string>();
        [Obfuscation] public string SelectDiameterCirclePilePara { get; set; }
        [Obfuscation] public List<Level> AllLevels { get; set; } = new List<Level>();
        [Obfuscation] public Level SelectedLevel { get; set; }

        [Obfuscation] public LanguageData SelectedLanguage {
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
        public bool IsModelByTwoLine {
            get => _isModelByTwoLine;
            set
            {
                _isModelByTwoLine = value;
                OnPropertyChanged();
            }
        }
        public bool IsModelByCircle
        {
            get => _isModelByCircle;
            set
            {
                _isModelByCircle = value;
                OnPropertyChanged();
            }
        }
        [Obfuscation] public string SelectedLayerHint { get; set; }
        [Obfuscation] public string TenModelByTwoLine { get; set; }
        [Obfuscation] public string PileTypeHint { get; set; }
        [Obfuscation] public string TenModelByCircle { get; set; }
        [Obfuscation] public string TenAllFamily { get; set; }
        [Obfuscation] public string TenDiameterCirclePilePara { get; set; }

        [Obfuscation] public string SelectedLevelHint { get; set; }


        #endregion public property
        public PileFromCADViewModel(UIDocument uidoc)
        {
            Doc = uidoc.Document;
            UiDoc = uidoc;

            Initialize();
            ChangeLanguage();
        }

        private void Initialize()
        {
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));
            _settingToSave = new SettingCreatePile("CreatePileFromCADSetting");
            _settingRepository = new SettingRepository<SettingCreatePile>();
            _latestSetting = _settingRepository.GetSetting(_settingToSave.DefautSettingPath);

            List<Type> typesFilter = new List<Type>()
            {
                typeof(ImportInstance),
            };
            try {
                SelectedCadLink = ElementSelector.PickObject(UiDoc, new ClassSelectionFilter(typesFilter), "Select a CAD link");
            }
            catch (Exception) {
                return;
            }

            CadInstance = SelectedCadLink as ImportInstance;

            AllLayers = CadUtils.GetAllLayer(CadInstance);

            if (AllLayers.Count != 0) {
                SelectedLayer = AllLayers[0];
            }

            AllPile = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .Where(e => e is FamilySymbol).Cast<FamilySymbol>().ToList();

            if (AllPile.Count != 0) {
                AllPile = AllPile.OrderBy(pile => pile.Name).ToList();
                SelectedPile = AllPile[0];
            }
            else {
                return;
            }

            Category category = Category.GetCategory(Doc, BuiltInCategory.OST_StructuralFoundation);
            AllFamilies = new FilteredElementCollector(Doc).OfClass(typeof(Family)).Cast<Family>()
                .Where(e => e.FamilyCategory.Id.Equals(category.Id)).ToList();

            if (AllFamilies.Count != 0) {
                AllFamilies = AllFamilies.OrderBy(x => x.Name).ToList();
                SelectedFamily = AllFamilies[0];
            }

            FamilySymbol first = AllFamilies[0].GetAllFamilySymbol().FirstOrDefault();
            if (first == null) {
                return;
            }

            AllPileTypeParameter = ParameterUtils.GetAllTypeParameters(first);
            AllPileTypeParameter = AllPileTypeParameter.Where(p => !p.Contains("Assembly"))
                .Where(p => !p.Contains("OmniClass"))
                .Where(p => !p.Contains("Material"))
                .Where(p => !p.Contains("Category"))
                 .Where(p => !p.Contains("Type")).ToList();

            if (AllPileTypeParameter.Count != 0) {
                SelectDiameterCirclePilePara = AllPileTypeParameter[0];
            }

            AllLevels = new FilteredElementCollector(Doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            AllLevels = AllLevels.OrderBy(l => l.Elevation).ToList();

            if (AllLevels.Count != 0) {
                SelectedLevel = AllLevels[0];
            }
            LoadSetting(this);
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng Cọc từ AutoCAD",
                "Model Pile from AutoCAD");
            SelectedLayerHint = BindingUtils.ChangeLanguage(_languageCode,
                "Layer Đường Tâm Cọc",
                "Center Line Layer of Pile");
            TenModelByTwoLine = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng bởi 2 Đường Thẳng",
                "Model by 2 Center Line");

            TenModelByCircle = BindingUtils.ChangeLanguage(_languageCode,
                "Dựng bởi Đường Biên Tròn",
                "Model by Boundary");

            TenAllFamily = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Family",
                "Choose Family");

            TenDiameterCirclePilePara = BindingUtils.ChangeLanguage(_languageCode,
                "Parameter Đường Kính Cọc",
                "Parameter of Pile Diameter");

            SelectedLevelHint = BindingUtils.ChangeLanguage(_languageCode,
                "Chọn Level Đặt Cọc",
                "Choose Level of Pipe");


            #endregion ChangeLanguage

            OnPropertyChangedAll(this);
        }

        private void LoadLatestSetting()
        {
            try {
                SelectedLayer = _latestSetting.SelectedLayer;
                try {
                    SelectedPile = AllPile.First(w => w.Id.GetValue().Equals(_latestSetting.SelectedPile));
                }
                catch (Exception ) { }

                try {
                    SelectedLevel = AllLevels.First(l => l.Id.GetValue().Equals(_latestSetting.SelectedLevel));
                }
                catch (Exception ) { }
            }
            catch (Exception) { }
        }

        internal void SaveSetting()
        {
            try {
                _settingToSave.SelectedLayer = SelectedLayer;
                try {
                    _settingToSave.SelectedPile = (int)SelectedPile.Id.GetValue();
                }
                catch (Exception) { }

                try {
                    _settingToSave.SelectedLevel = (int)SelectedLevel.Id.GetValue();
                }
                catch (Exception ) { }
                _settingRepository.SaveSetting(_settingToSave,_settingToSave.DefautSettingPath);
            }
            catch (Exception ) { }
        }
    }
}
