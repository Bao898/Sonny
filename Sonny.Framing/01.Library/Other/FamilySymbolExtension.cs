// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Autodesk.Revit.DB;

namespace SonnyBIM
{
    public class FamilySymbolExtension
    {
        public FamilySymbol FamilySymbol;

        [Obfuscation]
        public string FamilyAndType { get; set; }

        [Obfuscation]
        public string NameAndSymbolFamily { get; set; }

        public static string WordSpace { get; set; } = ": ";

        public FamilySymbolExtension(FamilySymbol symbol)
        {
            FamilySymbol = symbol;
            FamilyAndType = string.Concat(symbol.FamilyName, ": ", symbol.Name);
            string familyName = symbol.Family.Name;
            string familySymbolName = symbol.Name;
            NameAndSymbolFamily = familyName + ": " + familySymbolName;
        }
    }
}

