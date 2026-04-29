// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace SonnyBIM
{
    public class SettingCreateFoundation
    {
        public SettingCreateFoundation() { }

        /// <summary>
        /// Contructor with Name of file setting
        /// </summary>
        /// <param name="nameOfFileSetting">Name of file, not incule file extension, should with format xxxSetting</param>
        public SettingCreateFoundation(string nameOfFileSetting)
        {
            DefautSettingPath =
                string.Concat(SonnyBIMConstraint.SettingFolder,
                    string.Concat("\\", nameOfFileSetting, ".alb"));
        }

        [Obfuscation] public string DefautSettingPath { get; set; }
        [Obfuscation] public string SelectedLayer { get; set; }
        [Obfuscation] public long SelectedFloor { get; set; }
        [Obfuscation] public long SelectedLevel { get; set; }
    }
}
