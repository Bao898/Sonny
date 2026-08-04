// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using System.Reflection;

namespace SonnyBIM
{
    public class SettingCreatePile
    {
        public SettingCreatePile() { }

        /// <summary>
        /// Contructor with Name of file setting
        /// </summary>
        /// <param name="nameOfFileSetting">Name of file, not incule file extension, should with format xxxSetting</param>
        public SettingCreatePile(string nameOfFileSetting)
        {
            DefautSettingPath =
                string.Concat(SonnyBIMConstraint.SettingFolder,
                    string.Concat("\\", nameOfFileSetting, ".alb"));
        }

        [Obfuscation] public string DefautSettingPath { get; set; }
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public int SelectedPile { get; set; }
        [Obfuscation] public int SelectedLevel { get; set; }
    }
}
