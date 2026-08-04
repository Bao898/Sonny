// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM
{
    public static class SonnyBIMUnitUtils
    {
        private const double _eps = 1.0e-5;

        public static double Eps => _eps;

        public static double MinLineLength => _eps;

        public static double TolPointOnPlane => _eps;

        public static bool IsZero(double a, double tolerance) => tolerance > Math.Abs(a);

        public static bool IsZero(double a) => IsZero(a, _eps);

        public static bool IsEqual(double a, double b) => IsZero(b - a);


        #region Geometrical Comparison


        ///// <summary>
        ///// Compare two number a, b. Return 0 if a = b, return -1 if a smaller than b, return 1 if a > b
        ///// </summary>
        ///// <returns></returns>
        //public static int Compare(double a, double b) => IsEqual(a, b) ? 0 : (a < b ? -1 : 1);

        //public static int Compare(XYZ p, XYZ q)
        //{
        //    int d = Compare(p.X, q.X);

        //    if (0 == d)
        //    {
        //        d = Compare(p.Y, q.Y);

        //        if (0 == d)
        //        {
        //            d = Compare(p.Z, q.Z);
        //        }
        //    }
        //    return d;
        //}

        ////public static int Compare(Plane a, Plane b)
        ////{
        ////    int d = Compare(a.Normal, b.Normal);

        ////    if (0 == d)
        ////    {
        ////        d = Compare(a.SignedDistanceTo(XYZ.Zero),
        ////          b.SignedDistanceTo(XYZ.Zero));

        ////        if (0 == d)
        ////        {
        ////            d = Compare(a.XVec.AngleOnPlaneTo(b.XVec, b.Normal), 0);
        ////        }
        ////    }
        ////    return d;
        ////}

        //public static bool IsEqual(this XYZ p, XYZ q) => 0 == Compare(p, q);

        ///// <summary>
        ///// Return true if the given bounding box bb
        ///// contains the given point p in its interior.
        ///// </summary>
        //public static bool BoundingBoxXyzContains(BoundingBoxXYZ bb, XYZ p)
        //{
        //    return 0 < Compare(bb.Min, p) && 0 < Compare(p, bb.Max);
        //}


        ///// <summary>
        ///// Return true if the vectors v and w
        ///// are non-zero and perpendicular.
        ///// </summary>
        //public static bool IsPerpendicular(XYZ v, XYZ w)
        //{
        //    double a = v.GetLength();
        //    double b = v.GetLength();
        //    double c = Math.Abs(v.DotProduct(w));
        //    return _eps < a
        //      && _eps < b
        //      && _eps > c;
        //    // c * c < _eps * a * b
        //}

        //public static bool IsParallel(XYZ p, XYZ q) => p.CrossProduct(q).IsZeroLength();

        //public static bool IsHorizontal(XYZ v) => IsZero(v.Z);

        //public static bool IsHorizontal(Edge e)
        //{
        //    XYZ p = e.Evaluate(0);
        //    XYZ q = e.Evaluate(1);
        //    return IsHorizontal(q - p);
        //}

        //public static bool IsHorizontal(PlanarFace f) => IsVertical(f.FaceNormal);

        //public static bool IsVertical(XYZ v) => IsZero(v.X) && IsZero(v.Y);

        //public static bool IsVertical(XYZ v, double tolerance) => IsZero(v.X, tolerance)
        //                                                          && IsZero(v.Y, tolerance);

        //public static bool IsVertical(PlanarFace f) => IsHorizontal(f.FaceNormal);

        //public static bool IsVertical(CylindricalFace f) => IsVertical(f.Axis);

        ///// <summary>
        ///// Minimum slope for a vector to be considered
        ///// to be pointing upwards. Slope is simply the
        ///// relationship between the vertical and
        ///// horizontal components.
        ///// </summary>
        //private const double _minimumSlope = 0.3;

        ///// <summary>
        ///// Return true if the Z coordinate of the
        ///// given vector is positive and the slope
        ///// is larger than the minimum limit.
        ///// </summary>
        //public static bool PointsUpwards(XYZ v)
        //{
        //    double horizontalLength = v.X * v.X + v.Y * v.Y;
        //    double verticalLength = v.Z * v.Z;

        //    return 0 < v.Z
        //      && _minimumSlope
        //        < verticalLength / horizontalLength;

        //    //return _eps < v.Normalize().Z;
        //    //return _eps < v.Normalize().Z && IsVertical( v.Normalize(), tolerance );
        //}

        ///// <summary>
        ///// Return the maximum value from an array of real numbers.
        ///// </summary>
        //public static double Max(double[] a)
        //{
        //    Debug.Assert(1 == a.Rank, "expected one-dimensional array");
        //    Debug.Assert(0 == a.GetLowerBound(0), "expected zero-based array");
        //    Debug.Assert(0 < a.GetUpperBound(0), "expected non-empty array");
        //    double max = a[0];
        //    for (int i = 1; i <= a.GetUpperBound(0); ++i)
        //    {
        //        if (max < a[i])
        //        {
        //            max = a[i];
        //        }
        //    }
        //    return max;
        //}

        #endregion // Geometrical Comparison

        #region Unit Handling

        private const double _convertFeetToMillimeters = 12 * 25.4;
        private const double _convertFeetToMeter = _convertFeetToMillimeters * 0.001;
        private const double _convertFeetToDecimeters = _convertFeetToMillimeters * 0.01;
        private const double _convertFeetToCentimeters = _convertFeetToMillimeters * 0.1;

        private const double _convertCubicFeetToCubicMeter =
            _convertFeetToMeter * _convertFeetToMeter * _convertFeetToMeter;
        private const double _convertCubicFeetToCubicCentimeters =
            _convertFeetToCentimeters * _convertFeetToCentimeters * _convertFeetToCentimeters;
        private const double _convertSquareFeetToSquareMeter
            = _convertFeetToMeter * _convertFeetToMeter;

        // predefined constants for common angles
        private const double kPi = 3.14159265358979323846;
        /// <summary>
        ///     Convert a given length in millimetres to feet.
        /// </summary>
        public static double MmToFeet(this double mm)
        {
            return mm / _convertFeetToMillimeters;
        }

        public static double PixelToCm(double pixel)
        {
            return pixel * 0.0264583333;
        }

        public static double CmToPixel(double cm)
        {
            return cm / 0.0264583333;
        }

        public static double MmToInch(double mm)
        {
            return FeetToInch(MmToFeet(mm));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inch">inch </param>
        /// <returns></returns>
        public static double InchToFeet(double inch)
        {
            return inch * 0.0833333333;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="feet">foot</param>
        /// <returns></returns>
        public static double FeetToInch(double feet)
        {
            return feet * 12;
        }

        /// <summary>
        ///     Convert a given length in metres to feet.
        /// </summary>
        public static double MeterToFeet(double metter)
        {
            return metter / _convertFeetToMeter;
        }

        /// <summary>
        ///     Convert a given length in feet to millimetres.
        /// </summary>

        public static double FeetToMeter(double feet)
        {
            return feet * _convertFeetToMeter;
        }

        public static double FeetToDecimeters(double feet)
        {
            return feet * _convertFeetToDecimeters;
        }
        public static double FeetToCentimeters(double feet)
        {
            return feet * _convertFeetToCentimeters;
        }

        public static double FeetToMm(this double feet)
        {
            return Math.Round(feet * _convertFeetToMillimeters, 0);
        }


        /// <summary>
        /// Convert a given volume in feet to cubic meters.
        /// </summary>
        public static double CubicFeetToCubicMeter(double cubicFeet)
        {
            return cubicFeet * _convertCubicFeetToCubicMeter;
        }

        public static double CubicFeetToCubicCentimeters(double cubicFeet)
        {
            return cubicFeet * _convertCubicFeetToCubicCentimeters;
        }

        public static double CubicMeterToCubicFeet(double cubicMeter)
        {
            return cubicMeter / _convertCubicFeetToCubicMeter;
        }

        /// <summary>
        /// Convert a given volume in feet to cubic meters.
        /// </summary>
        public static double SquareFeetToSquareMeter(double squareFeet)
        {
            return squareFeet * _convertSquareFeetToSquareMeter;
        }
        public static double RadiansToDegrees(double rads)
        {
            return rads * (180.0 / kPi);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (kPi / 180.0);
        }


        #endregion

        #region Geometrical Calculation

        ///// <summary>
        ///// Return the midpoint between two points.
        ///// </summary>
        //public static XYZ Midpoint(XYZ p, XYZ q) => 0.5 * (p + q);

        ///// <summary>
        ///// Return the midpoint of a Line.
        ///// </summary>
        //public static XYZ Midpoint(Line line) => Midpoint(line.GetEndPoint(0),
        //    line.GetEndPoint(1));

        ///// <summary>
        ///// Return the normal of a Line in the XY plane.
        ///// </summary>
        //public static XYZ Normal(Line line)
        //{
        //    XYZ p = line.GetEndPoint(0);
        //    XYZ q = line.GetEndPoint(1);
        //    XYZ v = q - p;

        //    //Debug.Assert( IsZero( v.Z ),
        //    //  "expected horizontal line" );

        //    return v.CrossProduct(XYZ.BasisZ).Normalize();
        //}

        ///// <summary>
        ///// Return the bottom four XYZ corners of the given
        ///// bounding box in the XY plane at the minimum
        ///// Z elevation in the order lower left, lower
        ///// right, upper right, upper left:
        ///// </summary>
        //public static List<XYZ> GetBottomCorners(BoundingBoxXYZ b)
        //{
        //    double z = b.Min.Z;

        //    return new List<XYZ>() {
        //        new XYZ( b.Min.X, b.Min.Y, z ),
        //        new XYZ( b.Max.X, b.Min.Y, z ),
        //        new XYZ( b.Max.X, b.Max.Y, z ),
        //        new XYZ( b.Min.X, b.Max.Y, z )
        //    };
        //}

        ///// <summary>
        ///// Return the 2D intersection point between two
        ///// unbounded lines defined in the XY plane by the
        ///// start and end points of the two given curves.
        ///// By Magson Leone.
        ///// Return null if the two lines are coincident,
        ///// in which case the intersection is an infinite
        ///// line, or non-coincident and parallel, in which
        ///// case it is empty.
        ///// https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        ///// </summary>
        //public static XYZ Intersection(Curve c1, Curve c2)
        //{
        //    XYZ p1 = c1.GetEndPoint(0);
        //    XYZ q1 = c1.GetEndPoint(1);
        //    XYZ p2 = c2.GetEndPoint(0);
        //    XYZ q2 = c2.GetEndPoint(1);
        //    XYZ v1 = q1 - p1;
        //    XYZ v2 = q2 - p2;
        //    XYZ w = p2 - p1;

        //    XYZ p5 = null;

        //    double c = (v2.X * w.Y - v2.Y * w.X)
        //      / (v2.X * v1.Y - v2.Y * v1.X);

        //    if (!double.IsInfinity(c))
        //    {
        //        double x = p1.X + c * v1.X;
        //        double y = p1.Y + c * v1.Y;

        //        p5 = new XYZ(x, y, 0);
        //    }
        //    return p5;
        //}

        ///// <summary>
        ///// Create and return a solid sphere
        ///// with a given radius and centre point.
        ///// </summary>
        //public static Solid CreateSphereAt(XYZ centre, double radius)
        //{
        //    // Use the standard global coordinate system
        //    // as a frame, translated to the sphere centre.

        //    Frame frame = new Frame(centre, XYZ.BasisX,
        //      XYZ.BasisY, XYZ.BasisZ);

        //    // Create a vertical half-circle loop
        //    // that must be in the frame location.

        //    Arc arc = Arc.Create(
        //      centre - radius * XYZ.BasisZ,
        //      centre + radius * XYZ.BasisZ,
        //      centre + radius * XYZ.BasisX);

        //    Line line = Line.CreateBound(
        //      arc.GetEndPoint(1),
        //      arc.GetEndPoint(0));

        //    CurveLoop halfCircle = new CurveLoop();
        //    halfCircle.Append(arc);
        //    halfCircle.Append(line);

        //    List<CurveLoop> loops = new List<CurveLoop>(1);
        //    loops.Add(halfCircle);

        //    return GeometryCreationUtilities
        //      .CreateRevolvedGeometry(frame, loops,
        //        0, 2 * Math.PI);
        //}

        ///// <summary>
        ///// Create and return a cube of
        ///// side length d at the origin.
        ///// </summary>
        //public static Solid CreateCube(double d)
        //{
        //    return CreateRectangularPrism(
        //      XYZ.Zero, d, d, d);
        //}

        ///// <summary>
        ///// Create and return a rectangular prism of the
        ///// given side lengths centered at the given point.
        ///// </summary>
        //public static Solid CreateRectangularPrism(XYZ center, double d1, double d2, double d3)
        //{
        //    List<Curve> profile = new List<Curve>();
        //    XYZ profile00 = new XYZ(-d1 / 2, -d2 / 2, -d3 / 2);
        //    XYZ profile01 = new XYZ(-d1 / 2, d2 / 2, -d3 / 2);
        //    XYZ profile11 = new XYZ(d1 / 2, d2 / 2, -d3 / 2);
        //    XYZ profile10 = new XYZ(d1 / 2, -d2 / 2, -d3 / 2);

        //    profile.Add(Line.CreateBound(profile00, profile01));
        //    profile.Add(Line.CreateBound(profile01, profile11));
        //    profile.Add(Line.CreateBound(profile11, profile10));
        //    profile.Add(Line.CreateBound(profile10, profile00));

        //    CurveLoop curveLoop = CurveLoop.Create(profile);

        //    SolidOptions options = new SolidOptions(
        //      ElementId.InvalidElementId,
        //      ElementId.InvalidElementId);

        //    return GeometryCreationUtilities
        //      .CreateExtrusionGeometry(
        //        new CurveLoop[] { curveLoop },
        //        XYZ.BasisZ, d3, options);
        //}

        ///// <summary>
        ///// Create and return a solid representing
        ///// the bounding box of the input solid.
        ///// Assumption: aligned with Z axis.
        ///// Written, described and tested by Owen Merrick for
        ///// http://forums.autodesk.com/t5/revit-api-forum/create-solid-from-boundingbox/m-p/6592486
        ///// </summary>
        //public static Solid CreateSolidFromBoundingBox(Solid inputSolid)
        //{
        //    BoundingBoxXYZ bbox = inputSolid.GetBoundingBox();

        //    // Corners in BBox coords

        //    XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
        //    XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
        //    XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
        //    XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

        //    // Edges in BBox coords

        //    Line edge0 = Line.CreateBound(pt0, pt1);
        //    Line edge1 = Line.CreateBound(pt1, pt2);
        //    Line edge2 = Line.CreateBound(pt2, pt3);
        //    Line edge3 = Line.CreateBound(pt3, pt0);

        //    // Create loop, still in BBox coords

        //    List<Curve> edges = new List<Curve>();
        //    edges.Add(edge0);
        //    edges.Add(edge1);
        //    edges.Add(edge2);
        //    edges.Add(edge3);

        //    double height = bbox.Max.Z - bbox.Min.Z;

        //    CurveLoop baseLoop = CurveLoop.Create(edges);

        //    List<CurveLoop> loopList = new List<CurveLoop>();
        //    loopList.Add(baseLoop);

        //    Solid preTransformBox = GeometryCreationUtilities
        //      .CreateExtrusionGeometry(loopList, XYZ.BasisZ,
        //        height);

        //    Solid transformBox = SolidUtils.CreateTransformed(
        //      preTransformBox, bbox.Transform);

        //    return transformBox;
        //}

        ///// <summary>
        ///// Given two points and an axis, returns the
        ///// point with the greater value along the axis
        ///// </summary>
        ///// <param name="pt1">point 1 to compare</param>
        ///// <param name="pt2">point 2 to compare</param>
        ///// <param name="axis">axis to compare along</param>
        ///// <returns></returns>
        //public static XYZ
        //Greater(XYZ pt1, XYZ pt2, XYZ axis)
        //{
        //    XYZ pt = new XYZ();

        //    if (axis.Equals(XYZ.BasisX))
        //    {
        //        if (pt1.X > pt2.X)
        //            pt = pt1;
        //        else
        //            pt = pt2;
        //    }
        //    if (axis.Equals(XYZ.BasisY))
        //    {
        //        if (pt1.Y > pt2.Y)
        //            pt = pt1;
        //        else
        //            pt = pt2;
        //    }
        //    if (axis.Equals(XYZ.BasisZ))
        //    {
        //        if (pt1.Z > pt2.Z)
        //            pt = pt1;
        //        else
        //            pt = pt2;
        //    }

        //    return pt;
        //}

        ///// <summary>
        ///// given an array of pts, find the closest
        ///// pt to a given pt
        ///// </summary>
        //public static XYZ GetClosestPt(XYZ pt, List<XYZ> pts)
        //{
        //    XYZ closestPt = new XYZ();
        //    double closestDist = 0.0;

        //    foreach (XYZ ptTemp in pts)
        //    {
        //        /// don't consider the pt itself
        //        if (pt.Equals(ptTemp))
        //            continue;

        //        double dist = Math.Sqrt(Math.Pow((pt.X - ptTemp.X), 2.0) +
        //            Math.Pow((pt.Y - ptTemp.Y), 2.0) +
        //            Math.Pow((pt.Z - ptTemp.Z), 2.0));

        //        if (closestPt.IsZeroLength())
        //        {
        //            closestDist = dist;
        //            closestPt = ptTemp;
        //        }
        //        else
        //        {
        //            if (dist < closestDist)
        //            {
        //                closestDist = dist;
        //                closestPt = ptTemp;
        //            }
        //        }
        //    }
        //    return closestPt;
        //}

        #endregion // Geometrical XYZ Calculation

        #region Math support

        /// <summary>
        /// làm tròn inputNumber lên 50. Trả về đơn vị mm
        /// </summary>
        /// <param name="inputNumber">đơn vị: mm ví dụ:1235.5</param>
        /// <returns></returns>
        public static double LamTronLen50(double inputNumber)
        {
            inputNumber = Math.Round(inputNumber, 0);
            double value1 = Math.Floor(inputNumber / 100) * 100; // =1200
            double value2 = inputNumber - value1; // = 1235.5-1200=35.5
            int value3;
            if (Math.Abs(value2) < SonnyBIMConstraint.Tolerance)
            {
                value3 = 0;
            }
            else
            {
                value3 = value2 < 50 ? 50 : 100; //=50
            }

            return value1 + value3;// =1250
        }
        /// <summary>
        /// làm tròn inputNumber xuống 50.Trả về đơn vị mm
        /// </summary>
        /// <param name="inputNumber">đơn vị: mm, ví dụ: 1266.5</param>
        /// <returns></returns>
        public static double LamTrongXuong50(double inputNumber)
        {
            inputNumber = Math.Round(inputNumber, 0);

            double value1 = Math.Floor(inputNumber / 100) * 100; // =1200
            double value2 = inputNumber - value1; // = 1266.5-1200=66.5
            int value3;

            if (Math.Abs(value2) < SonnyBIMConstraint.Tolerance)
            {
                value3 = 0;
            }
            else
            {
                value3 = value2 < 50 ? 0 : 50; //=50
            }
            return value1 + value3;// =1250
        }

        /// <summary>
        /// Làm tròn number tới số gần nó nhất theo giá trị làm tròn increment
        /// ví dụ: number = 363.2; increment = 5 --> 365
        /// </summary>
        /// <param name="number"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static double RoundNearest(double number, double increment)
        {
            number = number + 0.1;
            double abs = Math.Abs(Math.Round(increment, 0) - increment);
            if (Math.Abs(abs) < 0.0001)
            {
                int round = (int)increment;
                int phanNguyen = Convert.ToInt32(number / round) * round;
                double phanDu = number - phanNguyen;
                int phanThem = 0;
                if (phanDu >= increment * 0.5) phanThem = round;
                return phanNguyen + phanThem;
            }
            else
            {
                if (increment == 0.1) return Math.Round(number, 1);
                if (increment == 0.01) return Math.Round(number, 2);
                if (increment == 0.001) return Math.Round(number, 3);
                if (increment == 0.0001) return Math.Round(number, 4);
                if (increment == 0.00001) return Math.Round(number, 5);
                if (increment == 0.000001) return Math.Round(number, 6);
                if (increment == 0.0000001) return Math.Round(number, 7);
            }
            return number;
        }

        #endregion


    }
}
