// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Media.Imaging;
using Autodesk.Revit.UI.Selection;

namespace SonnyBIM
{
    public class CategorySelectionFilter : ISelectionFilter
    {
        private List<ElementId> _categoryIds = new List<ElementId>();

        public CategorySelectionFilter(Category category)
        {
            _categoryIds.Add(category.Id);
        }

        public CategorySelectionFilter(IEnumerable<Category> categories)
        {
            _categoryIds =
                categories.Where(category => category != null)
                    .Where(category => category.Id!=ElementId.InvalidElementId)
                    .Select(category => category.Id).ToList();
        }

        public bool AllowElement(Element elem)
        {
            return elem.Category != null && _categoryIds.Contains(elem.Category.Id);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}

