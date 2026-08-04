// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace SonnyBIM;

public static class GeometryObjectUtils
{
    public static bool CheckSameLayer(this GeometryObject geometryObject, string layer, Document doc)
    {
        if (null ==  geometryObject || null == layer) {
            MethodBase.GetCurrentMethod().InforMethodException();
        }

        ElementId lineGraphicsStyleId = geometryObject.GraphicsStyleId;
        GraphicsStyle? graphicsStyle = doc.GetElement(lineGraphicsStyleId) as GraphicsStyle;
        if (graphicsStyle == null) {
            return false;
        }

        Category styleCategory = graphicsStyle.GraphicsStyleCategory;
        if (styleCategory.Name.Equals(layer)) {
            return true;
        }
        return false;
    }
}
