// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Sonny.RevitExtensions.Extensions.GeometryObjects.Curves;

namespace SonnyBIM
{
    public static class CurveUtils
    {
        /// <summary>
        /// Kiểm tra curveCheck có nằm hoàn toàn trên curveTarget
        /// </summary>
        /// <param name="curveCheck">curveCheck: Curve cần kiểm tra</param>
        /// <param name="curveTarget">curveTarget: Curve dùng để kiểm tra</param>
        /// <returns></returns>
        public static bool IsInsideEntire(this Curve curveCheck, Curve curveTarget)
        {
            try {
                XYZ start = curveCheck.GetEndPoint(0);
                XYZ end = curveCheck.GetEndPoint(1);
                return IsContains(curveTarget, start)
                       && IsContains(curveTarget, end);
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra curveCheck có nằm hoàn toàn trên một trong những đường curvesForChecking nào không?
        /// </summary>
        /// <param name="curveCheck">curveCheck: Curve cần kiểm tra</param>
        /// <param name="curvesForChecking">curvesForChecking: Curves dùng để kiểm tra</param>
        /// <returns></returns>
        public static bool IsInsideEntire(this Curve curveCheck, List<Curve> curvesForChecking)
        {
            foreach (var curveTarget in curvesForChecking) {
                if (IsInsideEntire(curveCheck, curveTarget))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="curve">curve</param>
        /// <param name="point">point</param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsContains(this Curve curve,
            XYZ point,
            double tolerance = 0.0001)
        {
            if (curve.IsBound) {
                try {
                    XYZ a = curve.GetEndPoint(0); // line start point
                    XYZ b = curve.GetEndPoint(1); // line end point
                    double f = a.DistanceTo(b); // distance between focal points
                    double da = a.DistanceTo(point);
                    double db = point.DistanceTo(b);
                    // da + db is always greater or equal f
                    //return (da + db - f) * f < tolerance;
                    return Math.Abs(da + db - f) < tolerance;
                }
                catch (Exception) {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Lấy về tập hợp gồm các tập hợp con mà mỗi tập con là những đường song song và cách nhau một khoảng d
        /// </summary>
        /// <param name="curves">curves: tất cả curve cần lọc</param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<List<Curve>> GetCurvesParallelAndDistance(List<Curve> curves, double d)
        {
            List<List<Curve>> result = new List<List<Curve>>();

            while (curves.Count > 1) {
                List<Curve> curvesParallel = new List<Curve>();
                Curve curveCheck = curves[0];
                curvesParallel.Add(curveCheck);
                curves = curves.Except(new[] { curveCheck }).ToList();

                foreach (Curve c in curves) {
                    bool isContainsStart = curveCheck.IsContains(curveCheck.ProjectionOf(c.GetEndPoint(0)));
                    bool isContainsEnd = curveCheck.IsContains(curveCheck.ProjectionOf(c.GetEndPoint(1)));
                    bool isContainsStart2 = c.IsContains(curveCheck.ProjectionOf(c.GetEndPoint(0)));
                    bool isContainsEnd2 = c.IsContains(curveCheck.ProjectionOf(c.GetEndPoint(1)));

                    if (!isContainsEnd && !isContainsStart && !isContainsStart2 && !isContainsEnd2) {
                        continue;
                    }

                    bool isParallel = IsParallel(curveCheck, c);
                    double distance = Distance(curveCheck, c.GetEndPoint(0));
                    double abs = Math.Abs(distance - d);

                    if (isParallel && abs < 0.001) {
                        curvesParallel.Add(c);
                        curves.Except(new[] { c }).ToList();
                    }
                }

                if (curvesParallel.Count > 1) {
                    result.Add(curvesParallel);
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra 2 đường curve song song.
        /// Hai đường thẳng song song khi góc giữa chúng bằng 0 hoặc 180 độ và khoảng cách lớn hơn 0
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool IsParallel(Curve c1, Curve c2)
        {
            if (c1.Direction() == null || c2.Direction() == null) return false;
            double angle = c1.Direction().AngleTo(c2.Direction());
            if (Math.Abs(angle) < 0.001 || Math.Abs(angle - Math.PI) < 0.001) {
                return c1.Distance(c2.GetEndPoint(0)) > 0;
            }

            return false;
        }

        /// <summary>
        /// khoảng cách từ điểm point tới một Curve, cách 2
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double Distance(this Curve curve, XYZ point)
        {
            XYZ projection = curve.ProjectionOf(point);
            return Math.Abs((point - projection).GetLength());
        }
        public static XYZ NormalParallelTo(this Curve curve1, Curve curve2)
        {
            // Lấy hình chiếu của điểm đầu thuộc firstCurve xuống secondCurve
            // Khi đó giá trị cần lấy là vecto hướng từ điểm đầu thuộc firstCurve tới điểm hình chiếu này

            return curve2
                .ProjectionOf(curve1.GetEndPoint(0))
                .Subtract(curve1.GetEndPoint(0)).Normalize();
        }
        /// <summary>
        /// Lấy về vecto đơn vị chỉ hướng của Curve. Hướng từ end0 tới end1
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static XYZ Direction(this Curve c)
        {
            return (c as Line)?.Direction.Normalize();
        }
    }
}


