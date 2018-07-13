using System;
using System.Collections.Generic;
using PackageVerification.AssemblyScanner.Data;

namespace PackageVerification.AssemblyScanner.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, this is a simple console app used to test the ProcessAssembly class.");

            //ProcessBinFolder();

            BuildDNNVersionCSV();

            Console.ReadKey();
        }

        private static void ProcessBinFolder()
        {
            Console.WriteLine("Please enter the path to the Assemblies you would like to scan.");

            var input = Console.ReadLine();

            var dir = "C:\\Users\\Nathan\\Downloads\\iFinity.UrlMaster_02.06.00_Install"; //"c:\\AssemblyTest";

            if (input != null && input.Length > 1)
            {
                dir = input;
            }

            Console.WriteLine("About to Process the following directory: " + dir);

            Console.WriteLine("Press any key to begin.");

            Console.ReadKey();

            Console.WriteLine("Running...");

            var assemblies = ProcessAssembly.ProcessDirectory(dir);

            foreach (var assembly in assemblies)
            {
                var output = "Name: " + assembly.Name;

                output += " Version: " + assembly.Version;

                if (assembly.Framework != null)
                {
                    output += " .NET Version: " + assembly.Framework.VersionName;
                }

                Console.WriteLine(output);

                if (assembly.References != null)
                {
                    foreach (var reference in assembly.References)
                    {
                        var refOutput = "     References: " + reference.Name;

                        if (reference.Version != null)
                        {
                            refOutput += " Version: " + reference.Version;
                        }
                        Console.WriteLine(refOutput);
                    }
                }

                //Console.WriteLine(assembly.Name + "," + assembly.Version);
            }
        }

        private static void BuildDNNVersionCSV()
        {
            Console.WriteLine("Press any key to begin.");

            Console.ReadKey();

            Console.WriteLine("Running...");

            var serializer = new Serialize();

            var dnnAssembliesColl = Data.DotNetNukeAssemblies.BuildDotNetNukeAssembliesCollection();

            Console.WriteLine("Finished building assembly list.");

            serializer.SerializeObject<List<DotNetNukeAssembliesInfo>>(dnnAssembliesColl, @"..\..\..\DNNPackages\collection.xml");

            Console.WriteLine("Saved Object");

            //Console.WriteLine("Loading save assemblies collection.");

            //var dnnAssembliesColl = serializer.DeSerializeObject<List<DotNetNukeAssembliesInfo>>(@"..\..\..\DNNPackages\collection.xml");

            Console.WriteLine("Collection loaded, now creating the .csv");

            Utilities.ExportDotNetNukeAssembliesToCSV(dnnAssembliesColl);

            Console.WriteLine("Finished .csv export.");
        }
    }
}
