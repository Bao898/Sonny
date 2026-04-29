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
    public static class InformationMethod
    {
        public static void InforMethodException(this MethodBase currentMethod, string str = null)
        {
            //Type myType = (typeof(BeamRebarCmd));
            //// Get the public methods.
            //MethodInfo[] myArrayMethodInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //ParameterInfo[] parameterInfos = myArrayMethodInfo[0].GetParameters();

            ParameterInfo[] parameters = currentMethod.GetParameters();

            string information = string.Format("Error {0}(", currentMethod.Name);
            foreach (ParameterInfo parameterInfo in parameters)
            {
                information += parameterInfo.ParameterType + " " + parameterInfo.Name + ",";
            }

            information += ") at " + currentMethod.ReflectedType.FullName;

            if (str != null)
            {
                information += "\n" + str;
            }

            throw new Exception(information);
        }
    }
}
