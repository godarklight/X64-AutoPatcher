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
            Console.WriteLine("KSP Path: " + kspPathDir);
            Console.WriteLine("GameData Path: " + gameDataDir);
            if (!CecilReflector.LoadCecil(kspPathDir))
            {
                Console.WriteLine("Mono.Cecil not found!");
                AskToExit();
                return;
            }
            Console.WriteLine("==========");
            if (!UserAcceptedMessage())
            {
                Console.WriteLine("You need to accept by typing 'yes'.");
                AskToExit();
                return;
            }
            Console.WriteLine("==========");
            AssemblyPatcher.PatchFiles(gameDataDir);
            AskToExit();
        }

        private static bool UserAcceptedMessage()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("WARNING: The 64 bit windows version of KSP is ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("unstable");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("!");
            Console.ResetColor();
            Console.Write("It is ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("highly recommended ");
            Console.ResetColor();
            Console.Write("to first try ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("-force-opengl");
            Console.ResetColor();
            Console.WriteLine(" with the 32bit version of KSP before attempting to use the 64bit version.");
            Console.WriteLine();
            Console.WriteLine("You may only use this program with the following restrictions:");
            Console.WriteLine("1. You will not redistribute the edited binaries.");
            Console.WriteLine("2. You will not ask the official modders for support.");
            Console.WriteLine();
            Console.ResetColor();
            Console.Write("If you accept these conditions, type ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("yes");
            Console.ResetColor();
            Console.WriteLine(" and then press enter");
            return (Console.ReadLine().ToLower() == "yes");
        }

        private static void AskToExit()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}

