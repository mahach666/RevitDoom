using System;
using System.IO;
using System.Reflection;

namespace RevitDoom.Utils
{
    internal class Resolver
    {
        public static void Resolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";
            string fullPath = Path.Combine(assemblyPath, assemblyName);

            if (File.Exists(fullPath))
            {
                return Assembly.LoadFrom(fullPath);
            }

            return null;
        }
    }
}
