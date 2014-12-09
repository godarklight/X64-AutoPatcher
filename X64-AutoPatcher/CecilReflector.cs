using System;
using System.IO;
using System.Reflection;

namespace X64AutoPatcher
{
    public class CecilReflector
    {
        public static bool LoadCecil(string kspPathDir)
        {
            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
            string monoCecilPath = Path.Combine(Path.Combine(kspPathDir, "KSP_Data"), "Managed");
            if (!Directory.Exists(monoCecilPath))
            {
                Console.WriteLine("Error loading Mono.Cecil from: " + monoCecilPath + ", path does not exist");
                return false;
            }
            string[] kspManagedFiles = Directory.GetFiles(monoCecilPath, "*", SearchOption.TopDirectoryOnly);
            foreach (string file in kspManagedFiles)
            {
                if (file.ToLowerInvariant().EndsWith(".dll"))
                {
                    if (file.ToLowerInvariant().Contains("cecil") || file.ToLowerInvariant().Contains("unityengine"))
                    {
                        Console.WriteLine("Loading: " + file);
                        Assembly.LoadFile(file);
                    }
                }
            }
            return true;
        }

        private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            //This will find and return the assembly requested if it is already loaded
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    Console.WriteLine("Resolved " + args.Name);
                    return assembly;
                }
            }
            return null;
        }
    }
}

