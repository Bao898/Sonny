// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Reflection;
using Autodesk.Revit.UI;

namespace SonnyBIM
{
    public class AutoJoinViewModel : ViewModelBaseNew
    {

        #region private variable

        string _languageCode = LanguageData.GetLanguageSetting();
        private LanguageData _selectedLanguage;
        private ObservableCollection<AutoJoinRules> allRules = new ObservableCollection<AutoJoinRules>();
        private bool _isEnabledScope = true;

        #endregion

        #region public property

        [Obfuscation]
        public List<string> AllCategoryPriority { get; set; }

        [Obfuscation]
        public List<string> AllCutCategory { get; set; }

        [Obfuscation]
        public List<AutoJoinRules> SelectedRules { get; set; } = new List<AutoJoinRules>();

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
        public ObservableCollection<AutoJoinRules> AllRules {
            get => allRules;
            set
            {
                allRules = value;
                OnPropertyChanged();
            }
        }

        [Obfuscation]
        public bool IsEnabledScope {
            get => _isEnabledScope;
            set
            {
                _isEnabledScope = value;
                OnPropertyChanged();
            }
        }


        #endregion


        public AutoJoinViewModel(UIDocument uidoc)
        {
            // Khởi tạo sự kiện(nếu có)

            // Lưu trữ data từ Revit
            Doc = uidoc.Document;
            UiDoc = uidoc;

            // Khởi tạo data cho WPF
            Initialize();

            // Get setting(nếu có)

            ChangeLanguage();
        }

        private void Initialize()
        {
            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "QUY TẮC JOIN TỰ ĐỘNG",
                "AUTO JOIN RULES");
            SelectedLanguage = AllLanguage.FirstOrDefault(item => item.Code.Equals(_languageCode));

            /* Ý TƯỞNG:
             * - Nếu chọn trước đối tượng thì những đối tượng dùng để chạy Join là những đối tượng đó.
             * - Nếu không chọn trước thì những đối tượng dùng để chạy Join là những đối tượng có trên View.
             *
             */

            List<Element> currentSelection = UiDoc.Selection.GetElementIds()
                .Select(elId => Doc.GetElement(elId)).ToList();

            if (currentSelection.Any()) {
                string priorityCategory = "";
                string joinWithCategory = AllJoinCategory.StructuralColumn;

                var idValue = currentSelection.FirstOrDefault(e => e.Category?.CategoryType == CategoryType.Model)
                    ?.Category?.Id.GetValue();

                if (idValue != null) {
                    int selectedCategoryId = (int)idValue;

                    switch (selectedCategoryId) {
                        case (int)BuiltInCategory.OST_StructuralFraming:
                            priorityCategory = AllJoinCategory.Beam;
                            break;
                    }

                    AutoJoinRules autoJoinRules = new AutoJoinRules(priorityCategory, joinWithCategory);
                    AllRules = new ObservableCollection<AutoJoinRules>();
                    AllRules.Add(autoJoinRules);
                }
            }
            else {
                LoadLatestSetting();
                if (!AllRules.Any()) {
                    AutoJoinRules autoJoinRules = new AutoJoinRules(AllJoinCategory.Beam, AllJoinCategory.StructuralColumn);
                    AllRules = new ObservableCollection<AutoJoinRules>();
                    AllRules.Add(autoJoinRules);
                }
            }

            AllCategoryPriority = AllJoinCategory.AllCategoryPriority;
            AllCutCategory = AllJoinCategory.AllCutCategory;
        }

        internal void RemoveRules()
        {
            // Nếu remove trực tiếp SelectedRules thì sẽ báo lỗi
            List<AutoJoinRules> ruleToRemove = new List<AutoJoinRules>(SelectedRules);
            foreach (AutoJoinRules r in ruleToRemove) {
                if (r == null) { continue; }
                AllRules.Remove(r);
            }
        }

        private void ChangeLanguage()
        {
            ChangeLanguageViewModelBaseNew(_languageCode);

            #region ChangeLanguage

            TitleCaption = BindingUtils.ChangeLanguage(_languageCode,
                "Auto Join Rules",
                "Auto Join Rules");

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
