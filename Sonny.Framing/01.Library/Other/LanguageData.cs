// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;


namespace SonnyBIM
{
    public class LanguageData
    {
        /// <summary>
        /// Tiếng việt, vi
        /// </summary>
        public const string CodeVi = "vi";

        /// <summary>
        /// Tiếng Anh, en
        /// </summary>
        public const string CodeEn = "en";

        /// <summary>
        /// Tiếng Nhật, ja
        /// </summary>
        public const string CodeJa = "ja";

        /// <summary>
        /// Tiếng Tây ba Nha, es
        /// </summary>
        public const string CodeSpa = "es";

        /// <summary>
        /// Tiếng Indonesia, id
        /// </summary>
        public const string CodeIndo = "id";

        /// <summary>
        /// Tiếng thái lan, th
        /// </summary>
        public const string CodeThailan = "th";


        /// <summary>
        /// Tiếng Khmer, km
        /// </summary>
        public const string CodeCambodian = "km";


        /// <summary>
        /// Tiếng Trung Quoc, zh
        /// </summary>
        public const string CodeTrung = "zh";

        /// <summary>
        /// Tiếng Han Quoc, ko
        /// </summary>
        public const string CodeHan = "ko";


        [Obfuscation] public string Name { get; set; }
        public string Code { get; set; }

        public LanguageData(string name, string code)
        {
            Name = name;
            Code = code;
        }


        /// <summary>
        /// "Vietnamese", "vi"
        /// </summary>
        public static Tuple<string, string> CodeVietnam = new("Vietnamese", CodeVi);

        /// <summary>
        /// "English", "en"
        /// </summary>
        public static Tuple<string, string> CodeEnglish = new("English", CodeEn);

        /// <summary>
        /// "Japan", "ja"
        /// </summary>
        public static Tuple<string, string> CodeJapan = new("Japanese", CodeJa);

        /// <summary>
        /// "Spanish", "es"
        /// </summary>
        public static Tuple<string, string> CodeSpanish = new("Spanish", CodeSpa);

        /// <summary>
        /// "Indonesian", "id"
        /// </summary>
        public static Tuple<string, string> CodeIndonesian = new("Indonesian", CodeIndo);

        /// <summary>
        /// "Thai", "th"
        /// </summary>
        public static Tuple<string, string> CodeThai = new("Thai", CodeThailan);

        /// <summary>
        /// "Cambodian", "km"
        /// </summary>
        public static Tuple<string, string> CodeKhmer = new("Cambodian (Khmer)", CodeCambodian);

        /// <summary>
        /// "Trung Quốc", "zh"
        /// </summary>
        public static Tuple<string, string> CodeChina = new("China", CodeTrung);

        /// <summary>
        /// "Hàn Quốc", "ko"
        /// </summary>
        public static Tuple<string, string> CodeHanQuoc = new("Korea", CodeHan);


        [Obfuscation] public static List<LanguageData> AllLanguage = new()
        {
            new LanguageData(CodeVietnam.Item1, CodeVietnam.Item2),
            new LanguageData(CodeEnglish.Item1, CodeEnglish.Item2),
            new LanguageData(CodeSpanish.Item1, CodeSpanish.Item2),
            new LanguageData(CodeKhmer.Item1, CodeKhmer.Item2),
            new LanguageData(CodeThai.Item1, CodeThai.Item2),
            new LanguageData(CodeIndonesian.Item1, CodeIndonesian.Item2),
            new LanguageData(CodeChina.Item1, CodeChina.Item2),
            new LanguageData(CodeJapan.Item1, CodeJapan.Item2),
            new LanguageData(CodeHanQuoc.Item1, CodeHanQuoc.Item2),
        };

        /// <summary>
        /// Lấy về Language Code đã được lưu ở Setting.
        /// Nếu null thì trả về giá trị mặc định là English
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageSetting()
        {
            SettingRepository<SonnySetting> settingRepository = new SettingRepository<SonnySetting>();
            string pathSetting = PathSetting.SonnyBIMSetting;
            SonnySetting latestSetting = settingRepository.GetSetting(pathSetting);
            string languageCode = latestSetting.SelectedLanguageCode;
            if (string.IsNullOrEmpty(languageCode)) languageCode = CodeEnglish.Item2;
            return languageCode;
        }
    }
}
