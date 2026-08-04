// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace SonnyBIM
{
    public class SettingCreateWall
    {
        public SettingCreateWall() { }

        /// <summary>
        /// Contructor with Name of file setting
        /// </summary>
        /// <param name="nameOfFileSetting">Name of file, not incule file extension, should with format xxxSetting</param>
        public SettingCreateWall(string nameOfFileSetting)
        {
            DefautSettingPath =
                string.Concat(SonnyBIMConstraint.SettingFolder,
                    string.Concat("\\", nameOfFileSetting, ".alb"));
        }

        [Obfuscation] public string DefautSettingPath { get; set; }
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public int SelectedWallType1 { get; set; }
        [Obfuscation] public int SelectedWallType2 { get; set; }
        [Obfuscation] public int SelectedWallType3 { get; set; }
        [Obfuscation] public bool ForThicknessType1 { get; set; }
        [Obfuscation] public string AllThicknessType1 { get; set; }
        [Obfuscation] public bool ForThicknessType2 { get; set; }
        [Obfuscation] public string AllThicknessType2 { get; set; }
        [Obfuscation] public bool ForThicknessType3 { get; set; }
        [Obfuscation] public string AllThicknessType3 { get; set; }
        [Obfuscation] public int BaseLevel { get; set; }
        [Obfuscation] public int TopLevel { get; set; }
        [Obfuscation] public bool IsCreateWallStructural { get; set; }
        [Obfuscation] public double BaseOffset { get; set; }
        [Obfuscation] public double TopOffset { get; set; }

    }
}
