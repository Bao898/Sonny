// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace SonnyBIM;

public class SonnySetting
{
    public SonnySetting() { }

    [Obfuscation]
    public string SelectedLanguageCode { get; set; }


    //public static void SaveSonnyBIMSetting(LanguageData selectedLanguage)
    //{
    //    SettingRepository<SonnyBIMSetting>
    //        settingRepository = new SettingRepository<SonnyBIMSetting>();
    //    string pathSetting = PathSetting.SonnyBIMSetting;

    //    SonnyBIMSetting
    //        latestSetting = settingRepository.GetSetting(pathSetting);

    //    latestSetting.SelectedLanguageCode = selectedLanguage?.Code;

    //    try
    //    {
    //        settingRepository.SaveSetting(latestSetting, pathSetting);
    //    }
    //    catch (Exception)
    //    {
    //    }
    //}

    public static void SaveSonnySetting(string languageCode)
    {
        SettingRepository<SonnySetting> settingRepository = new SettingRepository<SonnySetting>();
        SonnySetting latestSetting = settingRepository.GetSetting(PathSetting.SonnyBIMSetting);
        latestSetting.SelectedLanguageCode = languageCode;
        try
        {
            settingRepository.SaveSetting(latestSetting, PathSetting.SonnyBIMSetting);
        }
        catch (Exception)
        {
        }
    }
}
