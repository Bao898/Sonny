// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public class InforPileByTwoLineModelFromCad : InforPileModelFromCad
{
    /// <summary>
    /// curves phải là curveloop or polyline
    /// </summary>
    /// <param name="curves"></param>
    public InforPileByTwoLineModelFromCad(Line line)
    {
        Center = line.GetMiddlePoint();
        Line = line;
    }
    public Line Line { get; set; }
}
