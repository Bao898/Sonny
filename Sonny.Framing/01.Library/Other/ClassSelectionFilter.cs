// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Windows;
using System.Diagnostics;
using Autodesk.Revit.UI.Selection;

namespace SonnyBIM
{

    public class ClassSelectionFilter : ISelectionFilter
    {
        private List<Guid> _typeGuid = new List<Guid>();

        public ClassSelectionFilter(Type type)
        {
            _typeGuid.Add(type.GUID);
        }

        public ClassSelectionFilter(List<Type> types)
        {
            _typeGuid = types.Select(category => category.GUID).ToList();
        }

        public bool AllowElement(Element elem)
        {
            return  _typeGuid.Contains(elem.GetType().GUID);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }


    /// <summary>
    /// Allows picking Structural Framing and Structural Column and Walls.
    /// </summary>
    public class BeamWallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem == null) return false;

            if (elem is FamilyInstance fi && fi.Category != null)
            {
                int cat = fi.Category.Id.IntegerValue;
                if (cat == (int)BuiltInCategory.OST_StructuralFraming) return true;
                if (cat == (int)BuiltInCategory.OST_StructuralColumns) return true;
            }

            if (elem is Wall) return true;

            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            // PickObjects(ObjectType.Element) only needs AllowElement
            return false;
        }
    }
}

