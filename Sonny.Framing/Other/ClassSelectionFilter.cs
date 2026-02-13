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
}

