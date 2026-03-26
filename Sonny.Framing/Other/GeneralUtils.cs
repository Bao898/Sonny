// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Outline = Autodesk.Revit.DB.Outline;
using Floor = Autodesk.Revit.DB.Floor;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace SonnyBIM;

public static class GeneralUtils
{
    /// <summary>
    /// Lấy về tất cả Walls có BoundingBox "đụng" với BoundingBox của element
    /// </summary>
    /// <param name="element"></param>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static IList<Element> GetWallsBoundingBoxIntersectWith(this Element element, Document doc,
        bool isCurrentViewScope,
        bool isCurrentSelectionScope, List<ElementId> currentSelectionIds,
        bool isAllJoinElement)
    {
        FilteredElementCollector filteredElementCollector;
        if (isAllJoinElement) {
            filteredElementCollector = new FilteredElementCollector(doc);
        }
        else {
            if (isCurrentViewScope) {
                filteredElementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            }
            else if (isCurrentSelectionScope) {
                if (currentSelectionIds.Count > 0) {
                    filteredElementCollector = new FilteredElementCollector(doc, currentSelectionIds);
                }
                else {
                    MessageBox.Show("No element be selected.", SonnyBIMConstraint.MessageBoxCaption,
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return null;
                }
            }
            else // IsCurrentModelScope = true
            {
                filteredElementCollector = new FilteredElementCollector(doc);
            }
        }


        ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(Wall));

        // ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(element);

        BoundingBoxXYZ box = element.get_BoundingBox(element.Document.ActiveView);
        Outline outline = new Outline(box.Min, box.Max);

        // Lọc ra các element có BoundingBox "đụng" outline
        BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outline);


        LogicalAndFilter logicalAndFilter = new LogicalAndFilter(elementClassFilter, boundingBoxIntersectsFilter);

        List<Element> list = filteredElementCollector.WherePasses(logicalAndFilter)
            .Excluding(new List<ElementId> { element.Id })
            .ToList();

        //LogicalOrFilter logicalOrFilter =
        //    new LogicalOrFilter(elementIntersectsElementFilter, boundingBoxIntersectsFilter);

        // true: lọc lấy ra tất cả instance mà StructuralMaterialType không phải là Steel
        //StructuralMaterialTypeFilter materialTypeFilter = new StructuralMaterialTypeFilter(StructuralMaterialType.Steel, true);

        //List<ElementFilter> elementFilters = new List<ElementFilter> { elementClassFilter, logicalOrFilter };

        //LogicalAndFilter logicalAndFilter = new LogicalAndFilter(elementFilters);

        //List<Wall> list = new FilteredElementCollector(doc).WherePasses(logicalAndFilter).Cast<Wall>().ToList();

        ICollection<ElementId> joinedElements = JoinGeometryUtils.GetJoinedElements(doc, element);
        if (joinedElements.Count != 0) {
            List<Element> walls = new FilteredElementCollector(doc, joinedElements)
                .WherePasses(elementClassFilter).ToList();
            list.AddRange(walls);
        }

        return list;
    }
}
