using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.IO;

namespace GTAIVDowngrader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            // Path to folder with dlls
            string assemblyFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "bin");

            // Get the name and format it as a filename
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";
            string assemblyPath = Path.Combine(assemblyFolder, assemblyName);

            // If it exists, return it
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            // Otherwise return null
            return null;
        }
    }
}
