// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.DB;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace SonnyBIM
{
    public class BindingUtils
    {
        public static string ChangeLanguage(string languageCode,
            string textVietnam, string textEnglish,
            string textJapanese = "", string textSpanish = "",
            string textIndonesian = "", string textThai = "", string textKhmer = "", string textTrungQuoc = "",
            string textHanQuoc = "")
        {
            switch (languageCode) {
                case LanguageData.CodeVi:
                    return textVietnam;

                case LanguageData.CodeEn:
                    return textEnglish;

                case LanguageData.CodeJa:
                    return string.IsNullOrEmpty(textJapanese) ? textEnglish : textJapanese;

                case LanguageData.CodeSpa:
                    return string.IsNullOrEmpty(textSpanish) ? textEnglish : textSpanish;

                case LanguageData.CodeIndo:
                    return string.IsNullOrEmpty(textIndonesian) ? textEnglish : textIndonesian;

                case LanguageData.CodeThailan:
                    return string.IsNullOrEmpty(textThai) ? textEnglish : textThai;

                case LanguageData.CodeCambodian:
                    return string.IsNullOrEmpty(textKhmer) ? textEnglish : textKhmer;

                case LanguageData.CodeTrung:
                    return string.IsNullOrEmpty(textTrungQuoc) ? textEnglish : textTrungQuoc;

                case LanguageData.CodeHan:
                    return string.IsNullOrEmpty(textHanQuoc) ? textEnglish : textHanQuoc;

                default:
                    return textEnglish;
            }
        }
        //public static void ChangeLanguage(Collection<ResourceDictionary> resourceDictionaries,
        //    string nameTools, string languageCode)
        //{
        //    ResourceDictionary dictionary = null;
        //    foreach (ResourceDictionary di in resourceDictionaries)
        //    {
        //        if (di.Source.ToString().Contains(nameTools))
        //        {
        //            dictionary = di;
        //        }
        //    }

        //    resourceDictionaries.Remove(dictionary);

        //    // Load file ResourceDictionary mới
        //    try
        //    {
        //        dictionary = new ResourceDictionary
        //        {
        //            Source = new Uri($"pack://application:,,,/SonnyBIM;component/01.Lib/Resources/{nameTools}/{nameTools}.{languageCode}.xaml", UriKind.Absolute)
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dictionary = new ResourceDictionary
        //        {
        //            Source = new Uri($"pack://application:,,,/SonnyBIM;component/01.Lib/Resources/{nameTools}/{nameTools}.en.xaml", UriKind.Absolute)
        //        };
        //    }

        //    resourceDictionaries.Add(dictionary);
        //}
    }
}
