// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public class IEqualityComparerLineIntersect : IEqualityComparer<Curve>
{
    public bool Equals(Curve? c1, Curve? c2)
    {
        return CurveUtils.IsIntersection(c1, c2); // true: Loại bõ bớt 1 curve
    }

    public int GetHashCode(Curve obj)
    {
        return 0;
    }
}
