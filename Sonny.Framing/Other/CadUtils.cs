// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Sonny.Framing.SonnyStr;

namespace SonnyBIM
{
    public static class CadUtils
    {
        #region MyRegion

        ///// <summary>
        ///// Lấy về tất cả GeometryObject dạng Line(Line, Arc, PolyLine) của file cad link
        ///// </summary>
        ///// <param name="cadInstance"></param>
        ///// <param name="layerName"></param>
        ///// <returns></returns>
        //public static List<GeometryObject> GetCurveHaveName(ImportInstance cadInstance,
        //    string layerName)
        //{
        //    List<GeometryObject> result = new List<GeometryObject>();
        //    Options option = new Options();
        //    foreach (GeometryObject geoObject in cadInstance.get_Geometry(option))
        //    {
        //        if (geoObject is GeometryInstance)
        //        {
        //            GeometryInstance geoInstance = geoObject as GeometryInstance;

        //            // geo này có thể là Arc, Line, PolyLine, Point, Solid
        //            foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
        //            {
        //                if (geo is Solid) continue;

        //                GraphicsStyle graphicsStyle =
        //                    cadInstance.Document.GetElement(geo.GraphicsStyleId)
        //                    as GraphicsStyle;
        //                if (graphicsStyle == null) continue;
        //                Category styleCategory = graphicsStyle.GraphicsStyleCategory;

        //                if (styleCategory.Name.Equals(layerName))
        //                {
        //                    result.Add(geo);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        #endregion

        /// <summary>
        /// Lấy về chỉ Line, không bao gồm Arc. Bao gồm Line có trong Poly line. Đã loại bõ các Line trùng nhau
        /// </summary>
        /// <param name="cadInstance">cadInstance</param>
        /// <param name="layerName">layerName</param>
        /// <returns></returns>
        public static List<Curve> GetLineHaveName(ImportInstance cadInstance,
                   string layerName)
        {
            List<Curve> allCurve = new List<Curve>();
            Options option = new Options();
            foreach (GeometryObject geoObject in cadInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance? geoInstance = geoObject as GeometryInstance;

                    // geo này có thể là Arc, Line, PolyLine, Point, Solid
                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid) continue;

                        GraphicsStyle? graphicsStyle =
                            cadInstance.Document.GetElement(geo.GraphicsStyleId)
                            as GraphicsStyle;
                        if (graphicsStyle == null) continue;
                        Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                        if (styleCategory.Name.Equals(layerName))
                        {
                            if (geo is Line) allCurve.Add(geo as Line);
                            else if (geo is PolyLine)
                                allCurve.AddRange(GetLinesOfPolyLine(geo as PolyLine));
                        }
                    }
                }
            }

            // Loại bõ các Line trùng nhau
            foreach (var cv in allCurve)
            {
                List<Curve> allCurveToCheck = allCurve.Except(new[] { cv }).ToList();
                if (cv.IsInsideEntire(allCurveToCheck))
                    allCurve = allCurve.Except(new[] { cv }).ToList();
            }
            allCurve = allCurve.Distinct(new IEqualityLineEqual()).ToList();
            return allCurve;
        }


        /// <summary>
        /// Lấy về Line or Arc. Bao gồm Line có trong Poly line. Đã loại bõ các Line trùng nhau
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <param name="layerName">layerName</param>
        /// <returns></returns>
        public static List<Curve> GetCurveHaveName(
            ImportInstance importInstance,
            string layerName)
        {
            List<Curve> allCurve = new List<Curve>();
            Options option = new Options();
            foreach (GeometryObject geoObject in importInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInstance = geoObject as GeometryInstance;

                    // geo này có thể là Arc, Line, PolyLine, Point, Solid
                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid) continue;

                        GraphicsStyle? graphicsStyle =
                            importInstance.Document.GetElement(geo.GraphicsStyleId)
                            as GraphicsStyle;
                        if (graphicsStyle == null) continue;
                        Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                        if (styleCategory.Name.Equals(layerName))
                        {
                            if (geo is Line || geo is Arc) allCurve.Add(geo as Curve);
                            //if (geo is Curve) result.Add(geo as Curve);
                            else if (geo is PolyLine)
                                allCurve.AddRange(GetLinesOfPolyLine(geo as PolyLine));
                        }
                    }
                }
            }

            // Loại bõ các Line trùng nhau
            foreach (var cv in allCurve)
            {
                List<Curve> allCurveToCheck = allCurve.Except(new[] { cv }).ToList();
                if (cv.IsInsideEntire(allCurveToCheck))
                    allCurve = allCurve.Except(new[] { cv }).ToList();
            }
            allCurve = allCurve.Distinct(new IEqualityLineEqual()).ToList();

            return allCurve;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <param name="layerName">layerName</param>
        /// <returns></returns>
        public static List<CurveExtension> GetCurveMaxHaveName(ImportInstance importInstance,
                  string layerName)
        {
            List<CurveExtension> result = new List<CurveExtension>();
            Options option = new Options();
            foreach (GeometryObject geoObject in importInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance? geoInstance = geoObject as GeometryInstance;

                    // geo này có thể là Arc, Line, PolyLine, Point, Solid
                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid) continue;

                        GraphicsStyle? graphicsStyle =
                            importInstance.Document.GetElement(geo.GraphicsStyleId)
                            as GraphicsStyle;

                        if (graphicsStyle == null) continue;

                        Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                        if (styleCategory.Name.Equals(layerName))
                        {
                            if (geo is Line || geo is Arc) result.Add(new CurveExtension(geo as Curve, layerName));
                            else if (geo is PolyLine)
                            {
                                result.Add(new CurveExtension(GetLineMaxOfPolyLine(geo as PolyLine), layerName));
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Lấy về tất cả Solid có Faces.Count > 0 của file cad
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <returns></returns>
        public static List<Solid> GetSolids(ImportInstance importInstance)
        {
            string z = "0";
            List<Solid> result = new List<Solid>();
            Options option = new Options();
            foreach (GeometryObject geoObject in importInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance? geoInstance = geoObject as GeometryInstance;

                    // geo này có thể là Arc, Line, PolyLine, Point, Solid
                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid)
                        {
                            Solid solid = geo as Solid;
                            if (solid.Faces.Size > Convert.ToInt32(z)) result.Add(solid);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <param name="layerName">layerName</param>
        /// <returns></returns>
        public static List<PlanarFace> GetPlanarFaceHaveName(ImportInstance importInstance,
            string layerName)
        {
            List<PlanarFace> result = new List<PlanarFace>();

            List<Solid> solids = GetSolids(importInstance);

            if (solids.Count == Convert.ToDouble("0")) return result;

            foreach (Solid solid in solids)
            {
                foreach (PlanarFace face in solid.Faces)
                {
                    GraphicsStyle? graphicsStyle =
                        importInstance.Document.GetElement(face.GraphicsStyleId)
                           as GraphicsStyle;

                    if (graphicsStyle == null) continue;

                    Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                    if (styleCategory.Name.Equals(layerName))
                    {
                        result.Add(face);
                    }
                }
            }

            //result = result.Distinct(new IEqualityBoundingBoxUV()).ToList();
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <param name="includeSolid">includeSolid</param>
        /// <returns></returns>
        public static List<string> GetAllLayer(ImportInstance importInstance,
            bool includeSolid = false)
        {
            List<string> list = new List<string>();
            Options option = new Options();
            foreach (GeometryObject geoObject in importInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInstance = geoObject as GeometryInstance;

                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid)
                        {
                            if (includeSolid)
                            {
                                List<Solid> solids = GetSolids(importInstance);

                                if (solids.Count == Convert.ToDouble("0")) continue;

                                foreach (Solid solid in solids)
                                {
                                    foreach (PlanarFace face in solid.Faces)
                                    {
                                        GraphicsStyle? graphicsStyle =
                                            importInstance.Document.GetElement(face.GraphicsStyleId)
                                                as GraphicsStyle;

                                        if (graphicsStyle == null) continue;
                                        Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                                        list.Add(styleCategory.Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            GraphicsStyle? graphicsStyle =
                                importInstance.Document.GetElement(geo.GraphicsStyleId)
                              as GraphicsStyle;

                            if (graphicsStyle == null) continue;

                            Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                            list.Add(styleCategory.Name);
                        }
                    }
                }
            }

            List<string> result = list.Distinct().ToList();
            result.Sort();
            return result;
        }

        /// <summary>
        /// Lấy về tọa độ các đỉnh của các đường line có layer nhập vào
        /// </summary>
        /// <param name="importInstance">cadInstance</param>
        /// <param name="layerName">layerName</param>
        /// <returns></returns>
        public static List<XYZ> GetGetCoordinates(ImportInstance importInstance,
          string layerName)
        {
            List<XYZ> result = new List<XYZ>();
            Options option = new Options();
            foreach (GeometryObject geoObject in importInstance.get_Geometry(option))
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInstance = geoObject as GeometryInstance;

                    // geo này có thể là Arc, Line, PolyLine, Point, Solid
                    foreach (GeometryObject geo in geoInstance.GetInstanceGeometry())
                    {
                        if (geo is Solid) continue;

                        GraphicsStyle? graphicsStyle =
                            importInstance.Document.GetElement(geo.GraphicsStyleId)
                            as GraphicsStyle;

                        if (graphicsStyle == null) continue;

                        Category styleCategory = graphicsStyle.GraphicsStyleCategory;

                        if (styleCategory.Name.Equals(layerName))
                        {
                            if (geo is Line)
                            {
                                Line line = geo as Line;
                                result.Add(line.GetEndPoint(0));
                                result.Add(line.GetEndPoint(1));
                            }
                            if (geo is PolyLine)
                            {
                                PolyLine polyLine = geo as PolyLine;
                                result.AddRange(polyLine.GetCoordinates());
                            }

                            //if (geo is Arc)
                            //{
                            //    Arc arc = geo as Arc;
                            //    result.AddRange(arc.);
                            //}
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polyLine">polyLine</param>
        /// <returns></returns>
        public static List<Line> GetLinesOfPolyLine(PolyLine polyLine)
        {
            string z1 = "0";
            string z2 = "1";
            List<Line> result = new List<Line>();
            for (int i = Convert.ToInt32(z1);
                i < polyLine.NumberOfCoordinates - Convert.ToInt32(z2); i++)
            {
                Line line = null;

                try
                {
                    line = Line.CreateBound(polyLine.GetCoordinate(i),
                        polyLine.GetCoordinate(i + 1));
                    result.Add(line);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polyLine">polyLine</param>
        /// <returns></returns>
        public static Line? GetLineMaxOfPolyLine(PolyLine? polyLine)
        {
            string z1 = "0";
            string z2 = "1";
            List<Line?> result = new List<Line?>();
            for (int i = Convert.ToInt32(z1);
                i < polyLine.NumberOfCoordinates - Convert.ToInt32(z2); i++)
            {
                Line? line = null;

                try
                {
                    line = Line.CreateBound(polyLine.GetCoordinate(i), polyLine.GetCoordinate(i + 1));
                    result.Add(line);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (result.Count == 1) return result.First();

            double max = result.Max(l => l.Length);
            return result.FirstOrDefault(l => l.Length.Equals(max));
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="doc">doc</param>
        /// <param name="curve">curve</param>
        /// <returns></returns>
        public static string GetNameOfCurve(Document doc,
            Curve curve)
        {
            GraphicsStyle? graphicsStyle =
                doc.GetElement(curve.GraphicsStyleId)
                           as GraphicsStyle;

            // if (graphicsStyle == null) return string.Empty;

            Category styleCategory = graphicsStyle.GraphicsStyleCategory;

            return styleCategory.Name;
        }

    }
}

