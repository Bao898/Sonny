// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using View = Autodesk.Revit.DB.View;
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace SonnyBIM
{
    public class SonnyBIMConstraint
    {
        public static double Tolerance = SonnyBIMUnitUtils.MmToFeet(1);
        private static string sonnybim = "SonnyBIM";
        private static string aplicationPlugins = "ApplicationPlugins";
        private static string autodesk = "Autodesk";
        private static string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string sonnyBIM_SharedParameter = "SonnyBIM_SharedParameter";

        public string ContentsFolder;
        public string ResourcesFolder;
        public string HelpFolder;
        public string ImageFolder;
        public string RebarShapeFolder;

        /// <summary>
        /// SettingFolder  - zsomXBFVKVtvqsz
        /// </summary>
        public static string SettingFolder = Path.Combine(applicationData, autodesk, aplicationPlugins, sonnybim);

        public string DllFolder;
        public static string MessageBoxCaption = "SONNY BIM - SAVE YOUR TIME & MONEY";

        public static string Other32 = "other-32.png";
        public static string Other16 = "other-16.png";
        public string IconWindowICO;
        private string iconWindowPath;
        private Uri iconWindowUri;
        public BitmapImage IconWindow;
        public BitmapImage AboutIcon;

        /// <summary>
        /// Sonny BIM Plugins Guideline.pdf
        /// </summary>
        public string HelperPath;

        public string SharedParamsPath;
        public static string SharedParamsGroup_STR = "ALB_STR";
        public static string SharedParamsGroup_ARC = "ALB_ARC";
        public static string SharedParamsGroup_MEP = "ALB_MEP";

        public string RibbonTabName_GEN { get; set; }
        public string RibbonTabName_STR { get; set; }
        public string RibbonTabName_ARC { get; set; }
        public string RibbonTabName_MEP { get; set; }


        public SonnyBIMConstraint(ControlledApplication a = null)
        {
            ContentsFolder =
                "C:\\ProgramData\\Autodesk\\ApplicationPlugins\\SonnyBIM.bundle\\Contents";
            SharedParamsPath = Path.Combine(SettingFolder, sonnyBIM_SharedParameter);
            HelperPath = Path.Combine(ContentsFolder,
                "Resources", "Help", "Sonny BIM Plugins Guideline.pdf");
            ResourcesFolder = Path.Combine(ContentsFolder, "Resources");
            HelpFolder = Path.Combine(ResourcesFolder, "Help");
            ImageFolder = Path.Combine(ResourcesFolder, "Image");
            RebarShapeFolder = Path.Combine(ResourcesFolder, "RebarShape");

            IconWindowICO = "SonnyBIM.ico";
            iconWindowPath = Path.Combine(ImageFolder, IconWindowICO);
            iconWindowUri = new Uri(iconWindowPath, UriKind.Absolute);
            IconWindow = new BitmapImage(iconWindowUri);
            AboutIcon = new BitmapImage(new Uri(Path.Combine(ImageFolder, "SonnyBIM_520.png"),
                UriKind.Absolute));


            RibbonTabName_GEN = "SONNY BIM [G]";
            RibbonTabName_STR = "SONNY BIM [S]";
            RibbonTabName_ARC = "SONNY BIM [A]";
            RibbonTabName_MEP = "SONNY BIM [M]";


            if (a != null) {
                switch (a.VersionNumber) {
                    case "2017":
                        DllFolder = Path.Combine(ContentsFolder, "2017");
                        break;
                    case "2018":
                        DllFolder = Path.Combine(ContentsFolder, "2018");
                        break;
                    case "2019":
                        DllFolder = Path.Combine(ContentsFolder, "2019");
                        break;
                    case "2020":
                        DllFolder = Path.Combine(ContentsFolder, "2020");
                        break;
                    case "2021":
                        DllFolder = Path.Combine(ContentsFolder, "2021");
                        break;
                    case "2022":
                        DllFolder = Path.Combine(ContentsFolder, "2022");
                        break;
                    case "2023":
                        DllFolder = Path.Combine(ContentsFolder, "2023");
                        break;
                    case "2024":
                        DllFolder = Path.Combine(ContentsFolder, "2024");
                        break;
                    case "2025":
                        DllFolder = Path.Combine(ContentsFolder, "2025");
                        break;
                    case "2026":
                        DllFolder = Path.Combine(ContentsFolder, "2026");
                        break;
                    case "2027":
                        DllFolder = Path.Combine(ContentsFolder, "2027");
                        break;
                }
            }
        }
    }
}


