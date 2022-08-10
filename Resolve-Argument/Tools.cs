using ResolveArgument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Resolve_Argument
{
    internal class Tools
    {
        internal static void WriteResourcesToLog()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; i++)
            {
                LOGGER.Write($"Resource: {resourceNames[i]}");
            }
        }
    }
}
