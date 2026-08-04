// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public class InforPileByCircleModelFromCad : InforPileModelFromCad
{
    /// <summary>
    /// curves phải là curveloop or polyline
    /// </summary>
    /// <param name="curves"></param>
    internal InforPileByCircleModelFromCad(Arc arc)
    {
        Center = arc.Center;
        Diameter = arc.Radius * 2;
    }

    internal double Diameter { get; }
}
