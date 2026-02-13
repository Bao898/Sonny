// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using Quadrant = System.Int32;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using View = Autodesk.Revit.DB.View;
//using Microsoft.Office.Interop.Excel;
using Line = Autodesk.Revit.DB.Line;
using Autodesk.Revit.DB.Structure;
using SonnyBIM;

namespace SonnyBIM
{
    public class MethodSaveSetting
    {
        public static T ConvertDataSaveSeting<T>(object source, T target)
        {
            Type typeB = target.GetType();
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetIndexParameters().Length > 0))
                    continue;

                PropertyInfo other = typeB.GetProperty(property.Name);
                if ((other != null) && (other.CanWrite))
                {
                    if (property.PropertyType.Name != other.PropertyType.Name)
                    {
                        dynamic value = property.GetValue(source, null);

                        if (value != null)
                        {
                            if (value is FamilySymbolExtension)
                            {
                                FamilySymbolExtension extension = (FamilySymbolExtension)value;
                                if (extension.FamilySymbol != null)
                                    other.SetValue(target, extension.FamilySymbol.Name, null);
                            }
                            else if (value is System.Windows.Visibility)
                            {

                            }
                            else if (value is Autodesk.Revit.DB.Document)
                            {
                                Document doc = (Document)value;
                                if (doc != null)
                                    other.SetValue(target, doc.Title, null);
                            }
                            else
                            {
                                other.SetValue(target, value.Name, null);
                            }

                        }
                    }
                    else
                    {
                        other.SetValue(target, property.GetValue(source, null), null);
                    }
                }
            }

            return target;
        }




        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewModel">Chỉ để chạy OnpropertyChange</param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ConvertDataGetSeting<T>(dynamic viewModel, object source, ref T target)
        {
            Type typeB = target.GetType();
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                if (!property.CanRead || (property.GetIndexParameters().Length > 0))
                    continue;

                PropertyInfo other = typeB.GetProperty(property.Name);
                if ((other != null) && (other.CanWrite))
                {
                    if (property.PropertyType.Name != other.PropertyType.Name)
                    {
                        #region Không giải quyết đươc OnPropertyChanged không chịu thay đổi, không biết why

                        dynamic value = property.GetValue(source, null);

                        try
                        {
                            if (value != null)
                            {
                                if (other.PropertyType == typeof(RebarBarType))
                                {
                                    RebarBarType rebarBarType = new FilteredElementCollector(viewModel.Doc)
                                        .OfClass(typeof(RebarBarType))
                                        .Cast<RebarBarType>()
                                        .ToList().First(x => x.Name == value);

                                    other.SetValue(target, rebarBarType, null);
                                    viewModel.OnPropertyChanged(other.Name);
                                }
                                else if (other.PropertyType == typeof(FamilySymbol))
                                {
                                    FamilySymbol rebarBarType = new FilteredElementCollector(viewModel.Doc)
                                        .OfClass(typeof(FamilySymbol))
                                        .Cast<FamilySymbol>()
                                        .ToList().First(x => x.Name == value);

                                    other.SetValue(target, rebarBarType, null);
                                    viewModel.OnPropertyChanged(other.Name);
                                }
                                else if (other.PropertyType == typeof(ViewSheet))
                                {
                                    ViewSheet rebarBarType = new FilteredElementCollector(viewModel.Doc)
                                        .OfClass(typeof(ViewSheet))
                                        .Cast<ViewSheet>()
                                        .ToList().First(x => x.Name == value);

                                    other.SetValue(target, rebarBarType, null);
                                    viewModel.OnPropertyChanged(other.Name);
                                }
                            }

                        }
                        catch
                        {
                            // ignored
                        }

                        #endregion
                    }
                    else
                    {
                        other.SetValue(target, property.GetValue(source, null), null);
                    }
                }
            }
        }
        //public dynamic GetBarType(List<T> rebarBarTypes, string barType)
        //{
        //    foreach (dynamic type in rebarBarTypes)
        //    {
        //        if (type.Name == barType)
        //        {
        //            return type;
        //        }
        //    }
        //    return null;
        //}

        public object GetElementFromType<T>(List<T> rebarBarTypes, string barType)
        {
            foreach (dynamic type in rebarBarTypes)
            {
                if (type is FamilySymbolExtension)
                {
                    FamilySymbolExtension familySymbolExtension = (FamilySymbolExtension)type;
                    if (familySymbolExtension.FamilySymbol.Name == barType)
                    {
                        return type;
                    }
                }
                else if (type is Document)
                {
                    Document doc = (Document)type;
                    if (doc.Title == barType)
                    {
                        return type;
                    }
                }
                else
                {
                    if (type.Name == barType)
                    {
                        return type;
                    }
                }

            }
            return null;
        }

        public string GetElementName(dynamic ele)
        {
            if (ele is FamilySymbolExtension)
            {
                return ele.FamilySymbol.Name;
            }
            return ele.Name;
        }
    }
}

