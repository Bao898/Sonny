// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

internal class XyzExtension : IEquatable<XyzExtension>
{
    public XYZ XYZ { get; }

    internal XyzExtension(XYZ xyz)
    {
        XYZ = xyz;
    }

    public double X => XYZ.X;
    public double Y => XYZ.Y;
    public double Z => XYZ.Z;
    public bool Equals(XyzExtension other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        //bool b = X.Equals(other.X) &&
        //              Y.Equals(other.Y) &&
        //              Z.Equals(other.Z);

        //bool b = Math.Abs(X - other.X) < 1e-3 &&
        //         Math.Abs(Y - other.Y) < 1e-5 &&
        //         Math.Abs(Z - other.Z) < 1e-5;

        bool b = XYZ.IsAlmostEqualTo(other.XYZ);
        return b;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((XyzExtension)obj);
    }

    public override int GetHashCode()
    {
        return Tuple.Create(Math.Round(X, 10),
                Math.Round(Y, 10),
                Math.Round(Z, 10)).
            GetHashCode();
    }
}
