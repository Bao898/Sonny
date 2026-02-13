// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace SonnyBIM;

public static class ElementSelector
{
    /// <summary>
    /// Lấy về Element có BuiltInCategory là builtInCategory đụng với element
    /// </summary>
    /// <param name="element"></param>
    /// <param name="doc"></param>
    /// <returns></returns>

    public static List<Element> GetElementBoudingBoxIntersectWith(this Element element, Document doc,
        BuiltInCategory builtInCategory, bool isCurrentView, bool isCurrentSelection,
        List<ElementId> currentSelectionIds, double outLineScale = 1.2)
    {
        FilteredElementCollector collector = null;
        if (isCurrentView)
        {
            collector = new FilteredElementCollector(doc,doc.ActiveView.Id).WhereElementIsNotElementType();
        }
        else if(isCurrentSelection)
        {
            if (currentSelectionIds.Any())
            {
                collector = new FilteredElementCollector(doc,currentSelectionIds).WhereElementIsNotElementType();
            }

            else {
                return new List<Element>();
            }
        }
        else {
            collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
        }

        ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(builtInCategory);

        BoundingBoxXYZ box = element.get_BoundingBox(element.Document.ActiveView);
        Outline outLine = new Outline(box.Min, box.Max);
        outLine.Scale(outLineScale);
        BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outLine);
        List<ElementFilter> elementFilters = new List<ElementFilter>(){boundingBoxIntersectsFilter, elementCategoryFilter};

        LogicalAndFilter logicalAndFilter = new LogicalAndFilter(elementFilters);

        return collector.WherePasses(logicalAndFilter).Excluding(new List<ElementId>{element.Id}).ToList();
    }

    public static List<Element> GetElement(UIDocument uiDoc, List<BuiltInCategory> builtInCategories,
        bool isCurrentView, bool isCurrentSelection, List<ElementId> currentSelectionIds)
    {
        FilteredElementCollector collector = null;
        if (isCurrentView)
        {
            collector = new FilteredElementCollector(uiDoc.Document ,uiDoc.Document.ActiveView.Id).WhereElementIsNotElementType();
        }
        else if(isCurrentSelection)
        {
            if (currentSelectionIds.Any())
            {
                collector = new FilteredElementCollector(uiDoc.Document,currentSelectionIds).WhereElementIsNotElementType();
            }

            else
            {
                return new List<Element>();
            }
        }
        else {
            collector = new FilteredElementCollector(uiDoc.Document).WhereElementIsNotElementType();
        }

        ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(builtInCategories);
        return collector.WherePasses(elementMulticategoryFilter).WhereElementIsNotElementType().ToList();
    }

    /// <summary>
    /// Get all Element in current Selection that passed filter.
    /// </summary>
    /// <param name="uiDoc">The UIDocument.</param>
    /// <param name="selectionFilter">The ISelectionFilter.</param>
    /// <returns></returns>
    public static List<Element> GetAlreadySelectedElements(UIDocument uiDoc, ISelectionFilter selectionFilter = null)
    {
        List<Element> elementList = new List<Element>();
        HashSet<ElementId> elementIds = new HashSet<ElementId>(uiDoc.Selection.GetElementIds());
        foreach (ElementId elId in elementIds)
        {
            Element element = uiDoc.Document.GetElement(elId);
            if (selectionFilter == null)
            {
                elementList.Add(element);
                continue;
            }

            if (selectionFilter.AllowElement(element))
            {
                elementList.Add(element);
            }
        }
        return elementList;
    }

    /// <summary>
    /// Nếu chỉ có 1 đối tượng đang được chọn mà vượt qua bộ lọc thì sẽ trả về đối tượng đó.
    /// Nếu không có hoặc có nhiều 1 đối tượng đang được vượt qua bộ lọc thì sẽ pick lấy 1 đối tượng đầu tiên với bộ lọc chỉ định.
    /// </summary>
    /// <param name="uiDoc">The UIDocument.</param>
    /// <param name="selectionFilter">The selection filter.</param>
    /// <param name="statusPrompt">The message shown on the status bar.</param>
    /// <param name="useAlreadySelectedElements">Identifies if current selection is used.</param>
    /// <returns></returns>
    public static Element PickObject(UIDocument uiDoc, ISelectionFilter selectionFilter = null,
        string statusPromt = null, bool useAlreadySelectedElements = true)
    {
        Element element;
        Reference reference;
        try {
            List<Element> elementList = new List<Element>();
            if (useAlreadySelectedElements) {
                elementList = GetAlreadySelectedElements(uiDoc, selectionFilter);
            }

            if (elementList.Count == 1) {
                element = elementList[0];
            }
            else {
                reference = selectionFilter == null ? uiDoc.Selection.PickObject(ObjectType.Element, statusPromt)
                                                    : uiDoc.Selection.PickObject(ObjectType.Element, selectionFilter, statusPromt);
                element = uiDoc.Document.GetElement(reference);
            }
        }
        catch (Exception ex) {
            throw new Exception(ex.Message);
        }
        return element;
    }

    /// <summary>
    /// Nếu useAlreadySelectedElements = true: Lấy về các đối tượng được chọn hiện tại mà vượt qua bộ lọc.
    /// Nếu useAlreadySelectedElements = false: Pick các đối tượng với bộ lọc cho trước.
    /// </summary>
    /// <param name="uiDoc">The UIDocument.</param>
    /// <param name="selectionFilter">The selection filter.</param>
    /// <param name="statusPrompt">The message shown on the status bar.</param>
    /// <param name="useAlreadySelectedElements">Identifies if current selection is used.</param>
    /// <returns></returns>
    public static List<Element> PickObjects(UIDocument uiDoc, ISelectionFilter selectionFilter = null,
        string statusPromt = null, bool useAlreadySelectedElements = true)
    {
        List<Element> elementList = new List<Element>();
        if (uiDoc.ActiveView is ViewSchedule)
        {
            List<Element> allEles = new FilteredElementCollector(uiDoc.Document,uiDoc.ActiveView.Id).ToElements().ToList();

            if (selectionFilter == null)
            {
                return allEles;

            }
            return allEles.Where(e => selectionFilter.AllowElement(e)).ToList();
        }

        if (useAlreadySelectedElements) { elementList = GetAlreadySelectedElements(uiDoc, selectionFilter); }

        if (elementList.Count == 0)
        {
            if (selectionFilter == null)
            {
                foreach (Reference refe in uiDoc.Selection.PickObjects(ObjectType.Element,
                             statusPromt ?? string.Empty))
                {
                    elementList.Add(uiDoc.Document.GetElement(refe));
                }
            }
            else
            {
                foreach (Reference refe in uiDoc.Selection.PickObjects(ObjectType.Element, selectionFilter,
                             statusPromt ?? String.Empty))
                {
                    elementList.Add(uiDoc.Document.GetElement(refe));
                }
            }
        }
        return elementList;
    }

    public static Element PickObject(UIDocument uiDoc, BuiltInCategory builtInCategory, string statusPromt = null)
    {
        return PickObject(uiDoc, new CategorySelectionFilter(Category.GetCategory(uiDoc.Document, builtInCategory)), statusPromt);
    }

    public static List<Element> PickObjects(UIDocument uiDoc, List<BuiltInCategory> builtInCategory, string statusPromt = null)
    {
        List<Category> categoriesFilter = new List<Category>();
        foreach (var builtInCate in builtInCategory)
        {
            categoriesFilter.Add(Category.GetCategory(uiDoc.Document,builtInCate));
        }
        return PickObjects(uiDoc, new CategorySelectionFilter(categoriesFilter), statusPromt);
    }

    public static Element PickObject(UIDocument uiDoc, Type type, string statusPromt = null)
    {
        return PickObject(uiDoc, new ClassSelectionFilter(type), statusPromt);
    }

    public static List<Element> PickObjects(UIDocument uiDoc, Type type, string statusPromt = null)
    {
        return PickObjects(uiDoc, new ClassSelectionFilter(type), statusPromt);
    }

    public static List<Element> PickObjects(UIDocument uiDoc, List<Type> types, string statusPromt = null)
    {
        List<Type> listType = new List<Type>();
        foreach (var type in types)
        {
            listType.Add(type);
        }
        return PickObjects(uiDoc, new ClassSelectionFilter(listType), statusPromt);
    }
}
