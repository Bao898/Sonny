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

namespace SonnyBIM
{
    public class FileJson<T>
    {
        public T GetFileJson(string pathToSettingFile)
        {
            //string dllFolder = @"C:\ProgramData\Autodesk\ApplicationPlugins\SonnyBIM.bundle\Contents\2025";
            //AssemblyLoader.LoadAssemblies(dllFolder, "Newtonsoft.Json.dll");

            T result;
            using (StreamReader file = File.OpenText(pathToSettingFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                result = (T)serializer.Deserialize(file, typeof(T));
            }
            return result;
        }
        public void SaveFileJson(T fileToSave, string path)
        {
            //string dllFolder = @"C:\ProgramData\Autodesk\ApplicationPlugins\SonnyBIM.bundle\Contents\2025";
            //AssemblyLoader.LoadAssemblies(dllFolder, "Newtonsoft.Json.dll");

            if (!Directory.Exists(SonnyBIMConstraint.SettingFolder))
                OfficeUtils.CreateDirectory(SonnyBIMConstraint.SettingFolder);

            string json = JsonConvert.SerializeObject(fileToSave, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}

