using System;
using System.IO;
using System.Reflection;

namespace X64AutoPatcher
{
    public class CecilReflector
    {
        public static bool LoadCecil(string kspPathDir)
        {
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
    }
}

