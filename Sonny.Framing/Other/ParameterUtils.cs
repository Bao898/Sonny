// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM
{
    public static class ParameterUtils
    {
        public static List<string> GetAllTypeParameters(Element el)
        {
            List<string> result = new List<string>();
            if (el is ElementType)
            {
                foreach (Parameter p in el.Parameters) result.Add(p.Definition.Name);
            }
            else
            {
                Element elementType = el.Document.GetElement(el.GetTypeId());
                if (elementType != null)
                    foreach (Parameter p in elementType.Parameters)
                        result.Add(p.Definition.Name);
            }


            result = result.Distinct().ToList();
            result.Sort();
            return result;
        }

        /// <summary>
        /// Lấy về giá trị của parameter p
        /// </summary>
        /// <param name="p">Parameter cần lấy giá trị</param>
        /// <param name="asValueString">asValueString = true: Lấy giá trị parameter được hiện ra màn hình Revit</param>
        /// <returns></returns>
        public static string GetValue(this Parameter p,
            bool asValueString = false)
        {
            if (p == null) return String.Empty;
            if (asValueString && p.StorageType != StorageType.String)
                return p.AsValueString();

            switch (p.StorageType)
            {
                case StorageType.Double:
                    return p.AsDouble().ToString();

                case StorageType.ElementId:
                    try
                    {
#if ALB_R23 || ALB_R22 || ALB_R21
                return p.AsElementId().IntegerValue.ToString();
#else
                        return p.AsElementId().GetValue().ToString();
#endif

                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(e.ToString());
                        return String.Empty;
                    }

                case StorageType.Integer:
                    return p.AsInteger().ToString();

                case StorageType.String:
                    return p.AsString();

                case StorageType.None:
                    return p.AsValueString();
            }
            return String.Empty;
        }
    }
}


