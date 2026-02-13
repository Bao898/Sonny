// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using SonnyBIM;

namespace Sonny.Framing.SonnyStr
{
    public class SettingCreateFraming
    {
        public SettingCreateFraming() { }

        /// <summary>
        /// Contructor with Name of file setting
        /// </summary>
        /// <param name="nameOfFileSetting">Name of file, not incule file extension, should with format xxxSetting</param>
        public SettingCreateFraming(string nameOfFileSetting)
        {
            DefautSettingPath =
                string.Concat(SonnyBIMConstraint.SettingFolder,
                    string.Concat("\\", nameOfFileSetting, ".alb"));
        }

        [Obfuscation] public string DefautSettingPath { get; set; }
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public long SelectedFamilyFraming { get; set; }
        [Obfuscation] public string WidthParameter { get; set; }
        [Obfuscation] public string HeightParameter { get; set; }
        [Obfuscation] public long ReferenceLevel { get; set; }
        [Obfuscation] public double ZOffset { get; set; }
        [Obfuscation] public string AllSections { get; set; }
        [Obfuscation] public bool IsCreateForSingleLine { get; set; }
    }
}

