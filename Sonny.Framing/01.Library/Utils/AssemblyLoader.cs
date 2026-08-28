// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Reflection;

namespace SonnyBIM
{
    public class AssemblyLoader
    {
        public static void LoadAssemblies(string folderPath, string fileName)
        {
            var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name + ".dll" == fileName
                          || a.Location.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (alreadyLoaded)
                return;

            foreach (string assemblyFile in Directory.GetFiles(folderPath, "*.dll")) {
                if (!assemblyFile.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                    continue;
                Assembly.LoadFrom(assemblyFile);
                break;
            }
        }
    }



}
