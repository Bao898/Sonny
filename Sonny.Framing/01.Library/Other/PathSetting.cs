// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

#region Namespaces

using System.IO;

#endregion


namespace SonnyBIM
{
    public class PathSetting
    {
        /// <summary>
        /// PathUserLicense - Tivxxxxxxxxxxxxxxxxxxxxxxxx2021
        /// </summary>
        public static string PathUserLicense
            = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
              + "\\Autodesk\\ApplicationPlugins\\SonnyBIM\\";


        /// <summary>
        /// "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\SonnyBIMSetting.alb"
        /// </summary>
        public static string SonnyBIMSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "SonnyBIMSetting.alb");


        public static string ChangeMaterialSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "ChangeMaterialSetting2.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\FormworkAreaSetting.alb"
        /// </summary>
        public static string FormworkAreaSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "FormworkAreaSetting.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\SaveViewPointSetting.alb"
        /// </summary>
        public static string SaveViewPointSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "SaveViewPointSetting.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\ImportDWGSetting.alb"
        /// </summary>
        public static string ImportDWGSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "ImportDWGSetting.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\AutoJoinSetting.alb"
        /// </summary>
        public static string AutoJoinSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "AutoJoinSetting2.alb");

        public static string Create3DViewSetting
           = Path.Combine(SonnyBIMConstraint.SettingFolder, "Create3DViewSetting.alb");



        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\FoundationSetting.alb"
        /// </summary>
        public static string FoundationSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "FoundationRebarCmd.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\WallColumnRebarSetting.alb"
        /// </summary>
        public static string WallColumnRebarSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "WallColumnRebarSetting.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\BeamRebar.alb"
        /// </summary>
        public static string BeamRebar
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "BeamRebar.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\BeamRebarSetting.alb"
        /// </summary>
        public static string BeamRebarSetting
                = Path.Combine(SonnyBIMConstraint.SettingFolder, "BeamRebarSettingV2.alb");

        /// <summary>
        ///  "C:\Users\DUC\AppData\Roaming\Autodesk\ApplicationPlugins\SonnyBIM\ShopDrawingBeam.alb"
        /// </summary>
        public static string ShopDrawingBeamSetting
            = Path.Combine(SonnyBIMConstraint.SettingFolder, "ShopDrawingBeam.alb");


    }
}
