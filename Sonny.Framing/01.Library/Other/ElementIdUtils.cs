// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms.Design;
using Sonny.RevitExtensions.Extensions.Elements;
using Sonny.RevitExtensions.Extensions.GeometryObjects.Solids;

namespace SonnyBIM;

public static class ElementIdUtils
{
    public static long GetValue( this ElementId elementId )
    {
#if ALB_R23 || ALB_R22 || ALB_R21
      return elementId.IntegerValue ;
#else
        return elementId.Value ;
#endif
    }

    /// <summary>
    ///     Lấy về một khối solid có volumn lớn nhất trong các solid tạo nên element. Có thể trả về null
    /// </summary>
    /// <param name="element">element</param>
    /// <returns></returns>
    public static List<Solid> GetSolids(this Element element)
    {
        List<Solid> listSolids = new List<Solid>();
        Options options = new Options();
        options.ComputeReferences = true;
        GeometryElement geometry = element.get_Geometry(options);
        if (!(geometry is null)) {
            foreach (GeometryObject geometryObject in geometry) {
                if (geometryObject is Solid) {
                    Solid? solid = geometryObject as Solid;
                    if (solid.Volume > 1e-9) {
                        listSolids.Add(solid);
                    }
                }

                if (geometryObject is GeometryInstance) {
                    GeometryInstance? geometryInstance = geometryObject as GeometryInstance;
                    GeometryElement geometryElement = geometryInstance.GetInstanceGeometry();
                    foreach (GeometryObject o in geometryElement) {
                        if (o is Solid) {
                            Solid? solid = o as Solid;
                            if (solid.Volume > 1e-9) {
                                listSolids.Add(solid);
                            }
                        }
                    }
                }
            }
        }
        return listSolids;
    }

    /// <summary>
    /// V1
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static List<Curve> GetCurves(this Element element)
    {
        List<Curve> curves = new List<Curve>();
        List<Solid> solids = element.GetSolids();
        curves.AddRange(solids.GetCurves());

        GeometryElement geometryElement = element.get_Geometry(new Options());
        foreach (GeometryObject geometryObject in geometryElement) {
            if (geometryObject is GeometryInstance) {
                GeometryInstance?  geoInstance = geometryObject as GeometryInstance;
                GeometryElement geoElement2 = geoInstance.GetInstanceGeometry();
                foreach (GeometryObject geoObject2 in geoElement2) {
                    if (geoObject2 is Curve) {
                        curves.Add((Curve)geoObject2);
                    }
                }
            }

            if (geometryObject is Curve) {
                curves.Add((Curve)geometryObject);
            }
        }
        return curves;
    }
}
