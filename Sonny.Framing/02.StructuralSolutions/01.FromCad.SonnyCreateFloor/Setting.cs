// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using SonnyBIM;

namespace SonnyBIM
{
    public class SettingCreateFloor
    {
        public SettingCreateFloor() { }

        /// <summary>
        /// Contructor with Name of file setting
        /// </summary>
        /// <param name="nameOfFileSetting">Name of file, not incule file extension, should with format xxxSetting</param>
        public SettingCreateFloor(string nameOfFileSetting)
        {
            DefautSettingPath =
                string.Concat(SonnyBIMConstraint.SettingFolder,
                    string.Concat("\\", nameOfFileSetting, ".alb"));
        }

        [Obfuscation] public string DefautSettingPath { get; set; }
        [Obfuscation] public string SelectedLayerFloor { get; set; }
        [Obfuscation] public long SelectedFloor { get; set; }
        [Obfuscation] public long SelectedLevel { get; set; }
        [Obfuscation] public double LevelOffset { get; set; }
        [Obfuscation] public bool OnlyCreateBoundaryLine { get; set; }
        [Obfuscation] public bool FromHatch { get; set; }
        [Obfuscation] public bool CreateBoundaryLineOfHatch { get; set; }

    }
}
