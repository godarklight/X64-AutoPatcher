using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace X64AutoPatcher
{
    public class AssemblyPatcher
    {
        public static void PatchFiles(string gameDataPath)
        {
            string[] filesToCheck = Directory.GetFiles(gameDataPath, "*", SearchOption.AllDirectories);
            foreach (string fileToCheck in filesToCheck)
            {

                if (fileToCheck.ToLowerInvariant().EndsWith(".dll"))
                {
                    CheckFile(fileToCheck);
                }
            }
        }

        public static void CheckFile(string fileToCheck)
        {
            string fileName = Path.GetFileName(fileToCheck);
            Console.WriteLine("Checking: " + fileName);
            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(fileToCheck);
            bool assemblyChanged = false;
            foreach (TypeDefinition typeDefinition in assemblyDefinition.MainModule.Types)
            {
                bool typeChanged = false;
                FieldDefinition warningField = null;
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    if (methodDefinition.HasBody)
                    {
                        MethodBody methodBody = methodDefinition.Body;
                        ILProcessor processor = methodBody.GetILProcessor();
                        bool methodPatched = false;
                        foreach (Instruction instruction in methodBody.Instructions.ToArray())
                        {
                            if (instruction.OpCode == OpCodes.Call)
                            {
                                MethodReference methodReference = (MethodReference)instruction.Operand;
                                if ((methodReference.Name == "get_Size") && (methodReference.DeclaringType != null) && (methodReference.DeclaringType.FullName == "System.IntPtr"))
                                {
                                    Console.WriteLine("64 bit check found in " + typeDefinition.Name + "." + methodDefinition.Name + ", fixing...!");

                                    //Replace IntPtr.Size with 4
                                    Instruction newInstruction = Instruction.Create(OpCodes.Ldc_I4_4);
                                    processor.Replace(instruction, newInstruction);

                                    typeChanged = true;
                                    assemblyChanged = true;

                                    //Add warning
                                    if (warningField == null)
                                    {
                                        warningField = new FieldDefinition("X64LogWarningPrinted", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasFieldRVA, typeDefinition.Module.Import(typeof(bool)));
                                        warningField.InitialValue = BitConverter.GetBytes(false);
                                        typeDefinition.Fields.Add(warningField);
                                    }

                                    if (!methodPatched)
                                    {
                                        methodPatched = true;
                                        Instruction first = methodBody.Instructions[0];
                                        Instruction boolLoad = Instruction.Create(OpCodes.Ldsfld, warningField);
                                        Instruction boolJumpCall = Instruction.Create(OpCodes.Brtrue, first);
                                        Instruction loadTrue = Instruction.Create(OpCodes.Ldc_I4_1);
                                        Instruction boolSet = Instruction.Create(OpCodes.Stsfld, warningField);
                                        Instruction stringPush = Instruction.Create(OpCodes.Ldstr, "X64 PATCH WARNING TRIGGERED FOR FILE: " + fileName + ", METHOD: " + typeDefinition.Name + "." + methodDefinition.Name);
                                        Instruction consoleCall = Instruction.Create(OpCodes.Call, methodBody.Method.Module.Import(GetWriteLineReference()));
                                        processor.InsertBefore(first, boolLoad);
                                        processor.InsertAfter(boolLoad, boolJumpCall);
                                        processor.InsertAfter(boolJumpCall, loadTrue);
                                        processor.InsertAfter(loadTrue, boolSet);
                                        processor.InsertAfter(boolSet, stringPush);
                                        processor.InsertAfter(stringPush, consoleCall);
                                       

                                        /* Equivalent IL Code
                                            Load X64LogWarningPrinted onto the stack
                                            If bool is true, goto TheirCode
                                            Push true onto the stack
                                            Set X64LogWarningPrinted by popping the stack value
                                            Push "64 bit check found in " + typeName + "." + methodName + ", fixing...!" onto the stack;
                                            Call Console.WriteLine(string)
                                            //Their code with IntPtr.Size replaced with 4
                                        */

                                        /* Equivalent C# code
                                            if (!Type.X64LogWarningPrinted)
                                            {
                                                Type.X64LogWarningPrinted = true;
                                                UnityEngine.Debug.Log("64 bit check found in " + typeName + "." + methodName + ", fixing...!")
                                            }
                                            //Their code with IntPtr.Size replaced with 4
                                        */
                                    }


                                }
                            }
                        }
                        if (methodPatched)
                        {
                            //We need to calculate the position of the new instruction so jumps work correctly!
                            RecalculateOffsets(methodBody);
                        }
                    }
                }
                if (typeChanged)
                {
                    Console.WriteLine("Updated type: " + typeDefinition.Name);
                    assemblyDefinition.MainModule.Import(typeDefinition);
                }
            }
            if (assemblyChanged)
            {
                Console.WriteLine("Saving " + fileToCheck);
                File.Move(fileToCheck, fileToCheck + ".original");
                assemblyDefinition.Write(fileToCheck);
            }
        }

        private static void RecalculateOffsets(MethodBody methodBody)
        {
            int currentPos = 0;
            //Set previous and offsets
            Instruction previous = null;
            foreach (Instruction instruction in methodBody.Instructions)
            {
                instruction.Previous = previous;
                instruction.Offset = currentPos;
                currentPos += instruction.GetSize();
                previous = instruction;
            }
            //Set nexts
            Instruction[] flippedArray = methodBody.Instructions.ToArray();
            Array.Reverse(flippedArray);
            Instruction next = null;
            foreach (Instruction instruction in flippedArray)
            {
                instruction.Next = next;
                next = instruction;
            }
        }

        public static System.Reflection.MethodInfo GetWriteLineReference()
        {
            foreach (System.Reflection.Assembly testAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type testType in testAssembly.GetTypes())
                {
                    if (testType.Name == "Debug")
                    {
                        System.Reflection.MethodInfo[] logMethods = testType.GetMethods();
                        foreach (System.Reflection.MethodInfo logMethod in logMethods)
                        {
                            if (logMethod.Name == "Log" && logMethod.IsPublic && logMethod.GetParameters().Length == 1)
                            {
                                return logMethod;
                            }
                        }
                    }
                }
            }
            //For testing
            //return (typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));

            //We should error here
            return null;
        }
    }
}

