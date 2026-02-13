// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;
namespace SonnyBIM
{
    public class CurveExtension
    {
        public CurveExtension(Curve? curve, string name)
        {
            Curve = curve;
            Name = name;
        }

        public Curve? Curve { get; set; }
        public string Name { get; set; }
    }
}

