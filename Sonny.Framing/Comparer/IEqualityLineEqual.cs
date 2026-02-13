// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;

namespace SonnyBIM
{
    public class IEqualityLineEqual : IEqualityComparer<Curve>
    {
        public bool Equals(Curve c1, Curve c2)
        {
            IntersectionResultArray results;

            SetComparisonResult result
                = c1.Intersect(c2, out results);

            if (result == SetComparisonResult.Equal)
                return true;
            return false;
        }

        public int GetHashCode(Curve obj)
        {
            return 0;
        }
    }
}

