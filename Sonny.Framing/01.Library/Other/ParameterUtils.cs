// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Binding = Autodesk.Revit.DB.Binding;
using MessageBox = System.Windows.Forms.MessageBox;

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

        /// <summary>
        /// Lấy giá trị double, bõ phần đơn vị. Ví dụ: 2.4 m3 -> 2.4
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double GetDouble(string str)
        {
            char seperator = Convert.ToChar(" ");
            string[] strings = str.Split(seperator);
            double value = Double.MinValue;
            if (strings.Length > 1) {
                value = Convert.ToDouble(strings[0]);
            }
            else {
                value = Convert.ToDouble(str);
            }
            return value;
        }

        public static void CreateSharedParameter(Document doc, string sharedParamsGroup,
            string nameOfParameter, ForgeTypeId defType, ForgeTypeId displayGroup,
            string description, List<Category> categories, bool isInstanceParameter = true,
            bool visible = true, bool userModifiable = true)
        {
            DefinitionFile spFile = null;
            try {
                spFile = doc.Application.OpenSharedParameterFile();
            }
            catch { return;}

            if (spFile == null) {
                string sharedParamsPath = OfficeUtils.GetTempFile();

                spFile = GetSharedParamsFile(doc.Application, sharedParamsGroup);
                if (spFile == null) {
                    return;
                }
            }

            DefinitionGroup group = GetOrCreateSharedParamsGroup(spFile, sharedParamsGroup);

            if (group == null) {
                string sharedParamsPath = OfficeUtils.GetTempFile();
                spFile = GetSharedParamsFile(doc.Application, sharedParamsPath);
                CreateSharedParameter(doc, sharedParamsGroup, nameOfParameter, defType, displayGroup, description, categories);
                return;
            }

            Definition definition = GetOrCreateSharedParamsDefinition(group, nameOfParameter, defType,
                description,visible, userModifiable);

            if (isInstanceParameter) {
                CreateInstanceBindingParameter(doc, categories, definition, displayGroup);
            }
            else {
                CreateTypeBindingParameter(doc, categories, definition, displayGroup);
            }
        }

        private static DefinitionFile GetSharedParamsFile(Application app, string sharedParamsPath)
        {
            if (String.IsNullOrEmpty(sharedParamsPath)) {
                return null;
            }

            app.SharedParametersFilename = sharedParamsPath;

            // Lấy về file Shared Parameter
            DefinitionFile sharedParametersFile ;
            try {
                sharedParametersFile = app.OpenSharedParameterFile();
            }
            catch (Exception) {
                string languageCode = LanguageData.GetLanguageSetting();
                string text = BindingUtils.ChangeLanguage(languageCode,
                    "Vui lòng tắt hộp thoại hiện ra sau đây và chạy lại tool!",
                    "Please close the next dialog box then run this plugin again!");

                MessageBox.Show(text , SonnyBIMConstraint.MessageBoxCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                sharedParametersFile = null;
            }
            return sharedParametersFile;
        }

        private static DefinitionGroup GetOrCreateSharedParamsGroup(DefinitionFile sharedParameterFile,
            string grpoupName)
        {
            DefinitionGroup g = sharedParameterFile.Groups.get_Item(grpoupName);
            if (g == null) {
                try {
                    g = sharedParameterFile.Groups.Create(grpoupName);
                }
                catch (Exception) {
                    g = null;
                }
            }
            return g;
        }

        private static Definition GetOrCreateSharedParamsDefinition(DefinitionGroup defGroup,
            string nameOfParameter, ForgeTypeId defType, string description,
            bool visible = true, bool userModifiable = true)
        {
            Definition definition = defGroup.Definitions.get_Item(nameOfParameter);
            if (null == definition) {
                try {
                    ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(nameOfParameter, defType);

                    // True if the parameter is visible to the user,
                    // false if it is hidden and accessible only via the API. The default is true.
                    opt.Visible = visible;
                    opt.UserModifiable = userModifiable;
                    opt.Description = description;
                    definition = defGroup.Definitions.Create(opt);
                }
                catch (Exception ) {
                    definition = null;
                }
            }
            return definition;
        }

        private static void CreateInstanceBindingParameter(Document doc, List<Category> categories,
            Definition paramDef, ForgeTypeId displayGroup)
        {
            // Create the category set for binding and add the category
            CategorySet catSet = doc.Application.Create.NewCategorySet();
            foreach (Category cat in categories) {
                if (!cat.AllowsBoundParameters) { continue; }
                catSet.Insert(cat);
            }

            using (SubTransaction trans = new SubTransaction(doc)) {
                trans.Start();
                // Bind the param
                try {
                    Binding binding = doc.Application.Create.NewInstanceBinding(catSet);
                    doc.ParameterBindings.Insert(paramDef,binding,displayGroup);
                    trans.Commit();
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                    trans.RollBack();
                }
            }
        }

        private static void CreateTypeBindingParameter(Document doc, List<Category> categories,
            Definition paramDef, ForgeTypeId displayGroup)
        {
            // Create the category set for binding and add the category
            CategorySet catSet = doc.Application.Create.NewCategorySet();
            foreach (Category cat in categories) {
                if (!cat.AllowsBoundParameters) { continue; }
                catSet.Insert(cat);
            }

            using (SubTransaction trans = new SubTransaction(doc)) {
                trans.Start();
                // Bind the param
                try {
                    Binding binding = doc.Application.Create.NewTypeBinding(catSet);
                    // We could check if already bound, but looks like Insert will just ignore it in such case
                    doc.ParameterBindings.Insert(paramDef,binding,displayGroup);
                    trans.Commit();
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                    trans.RollBack();
                }
            }
        }
    }
}


