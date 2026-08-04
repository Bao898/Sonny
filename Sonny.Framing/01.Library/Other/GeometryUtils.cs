// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public static class GeometryUtils
{
    /// <summary>
    ///     Get Direction of Location currve of Wall/Beam
    ///     Trả về null khi dầm được dựng bằng model in place
    /// </summary>
    /// <param name="element"></param>
    public static XYZ GetDirection(this Element element)
    {
        LocationCurve locationCurve
            = element.Location as LocationCurve;

        if (locationCurve == null) return null;
        return locationCurve.Curve.Direction()?.Normalize();
    }
}
