using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace X64AutoPatcher
{
    public class MainClass
    {
        public static void Main()
        {
            string ourPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string ourPathName = new DirectoryInfo(ourPath).Name;
            bool isInKSPDir = Directory.Exists(Path.Combine(ourPath, "GameData"));
            bool isInGameDataDir = (ourPathName == "GameData");
            if (!isInKSPDir && !isInGameDataDir)
            {
                Console.WriteLine("Please place this executable in KSP's root folder or GameData folder.");
                AskToExit();
                return;
            }
            string kspPathDir = ourPath;
            string gameDataDir = ourPath;
            if (isInKSPDir)
            {
                gameDataDir = Path.Combine(ourPath, "GameData");
            }
            if (isInGameDataDir)
            {
                kspPathDir = Directory.GetParent(ourPath).FullName;
            }
            Console.WriteLine(kspPathDir);
            Console.WriteLine(gameDataDir);
            if (!CecilReflector.LoadCecil(kspPathDir))
            {
                Console.WriteLine("Mono.Cecil not found!");
                AskToExit();
                return;
            }
            AssemblyPatcher.PatchFiles(gameDataDir);
            AskToExit();
        }

        private static void AskToExit()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}

