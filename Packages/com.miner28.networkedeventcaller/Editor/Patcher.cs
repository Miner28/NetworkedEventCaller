using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Miner28.UdonUtils;
using Miner28.UdonUtils.Network;
using UdonSharp.Compiler;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Data;
using Debug = UnityEngine.Debug;

namespace Miner28.UdonUtils.Network.Editor
{
    [InitializeOnLoad]
    public class Patcher
    {
        internal static DataDictionary methodInfos;
        internal static DataDictionary filteredMethodInfos;

        static Harmony harmony;

        static Patcher()
        {
            harmony = new Harmony("Miner28.UdonUtils.Network");
            methodInfos = new DataDictionary();
            filteredMethodInfos = new DataDictionary();
            StartPatching();
        }

        private static void StartPatching()
        {
            Debug.Log("[NetworkEventCaller] Patching UdonSharp Compiler");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (IsUdonSharpAssembly(assembly))
                {
                    PatchUdonSharp(assembly);
                }
            }
        }

        private static void PatchUdonSharp(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.FullName.Equals("UdonSharp.Compiler.CompilationContext"))
                {
                    var method = type.GetMethod("BuildMethodLayout", BindingFlags.NonPublic | BindingFlags.Instance);
                    var patch = typeof(Patcher).GetMethod(nameof(BuildMethodLayoutPostfix),
                        BindingFlags.NonPublic | BindingFlags.Static);
                    harmony.Patch(method, null, new HarmonyMethod(patch));

                    var type2 = typeof(UdonSharpCompilerV1);
                    var method2 = type2.GetMethod("Compile", BindingFlags.Static | BindingFlags.Public);
                    var patch2 =
                        typeof(Patcher).GetMethod(nameof(PrefixOnCompile), BindingFlags.NonPublic | BindingFlags.Static);
                    harmony.Patch(method2, new HarmonyMethod(patch2), null);

                    var method3 = type2.GetMethod("CleanupCompile", BindingFlags.NonPublic | BindingFlags.Static);
                    var patch3 = typeof(Patcher).GetMethod(nameof(PostfixCompileFinished),
                        BindingFlags.NonPublic | BindingFlags.Static);
                    harmony.Patch(method3, null, new HarmonyMethod(patch3));

                    return;
                }
            }
        }


        private static void BuildMethodLayoutPostfix(object __result)
        {
            var type = __result.GetType();

            var methodProperty = type.GetProperty("Method", BindingFlags.Public | BindingFlags.Instance);
            var methodNameProperty = type.GetProperty("ExportMethodName", BindingFlags.Public | BindingFlags.Instance);
            var parametersProperty =
                type.GetProperty("ParameterExportNames", BindingFlags.Public | BindingFlags.Instance);

            var method = methodProperty.GetValue(__result).ToString();
            var methodName = methodNameProperty.GetValue(__result) as string;
            var parameters = parametersProperty.GetValue(__result) as string[];

            if (methodName == null || parameters == null || methodName == ".ctor")
                return;

            methodInfos.Add(method, new DataToken(new DataDictionary()
            {
                {"methodName", methodName},
                {"parameters", parameters.Length == 0 ? null : parameters.ToDataList()}
            }));

        }

        private static void PrefixOnCompile()
        {
            Debug.Log("[NetworkEventCaller] Compile Started - Clearing methodInfos");
            methodInfos.Clear();
        }

        private static void PostfixCompileFinished()
        {
            Debug.Log("[NetworkEventCaller] Compile Finished - Exporting methodInfos");
            VRCJson.TrySerializeToJson(methodInfos, JsonExportType.Beautify, out var json);

            //Write to file
            var path = Application.dataPath + "/../methodInfos.json";
            System.IO.File.WriteAllText(path, json.String);

            FilterMethodInfos();
        }
        
        internal static void LoadMethodInfos()
        {
            var path = Application.dataPath + "/../methodInfos.json";
            var json = System.IO.File.ReadAllText(path);
            VRCJson.TryDeserializeFromJson(json, out var data);
            methodInfos = data.DataDictionary;
        }


        internal static void FilterMethodInfos()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var methods = GetNetworkedMethods();
            filteredMethodInfos.Clear();

            foreach (var methodInfo in methods)
            {
                var method = methodInfo.GetFullName();
                if (methodInfos.ContainsKey(method))
                {
                    try
                    {
                        filteredMethodInfos.Add($"{methodInfo.DeclaringType}.{methodInfo.Name}", methodInfos[method]);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogError($"[NetworkEventCaller] Duplicate Networked Method: {methodInfo.Name} is this an overload? Overloads are not supported! If this is a class inheriting from another class with Networked Methods, you may ignore this error.");
                    }
                }
            }
            
            stopwatch.Stop();
            Debug.Log(
                $"[NetworkEventCaller] Found {methods.Count} Networked Methods in {stopwatch.ElapsedMilliseconds}ms");
            
            VRCJson.TrySerializeToJson(filteredMethodInfos, JsonExportType.Beautify, out var json);
            
            var path = Application.dataPath + "/../methodInfosFiltered.json";
            System.IO.File.WriteAllText(path, json.String);
        }

        private static List<MethodInfo> GetNetworkedMethods()
        {
            var methods = new List<MethodInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic || assembly.Location.Length <= 0 ||
                    assembly.Location.StartsWith("data"))
                    continue;

                if (assembly.GetName().Name == "Assembly-CSharp-Editor")
                    continue;

                if (IsUdonSharpAssembly(assembly))
                    continue;

                var assemblyTypes = assembly.GetTypes();
                foreach (var type in assemblyTypes)
                {
                    if (!typeof(NetworkInterface).IsAssignableFrom(type))
                        continue;
                    var typeMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.GetCustomAttributes(typeof(NetworkedMethod), false).Length > 0)
                        .ToList();
                    methods.AddRange(typeMethods);
                }
            }

            return methods;
        }

        private static bool IsUdonSharpAssembly(Assembly assembly)
        {
            return assembly.FullName.Contains("UdonSharp.Editor");
        }
    }


    public static class Extensions
    {
        public static string GetFullName(this MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterNames = parameters
                .Select(p => p.ParameterType.GetTypeName()).ToList();
            var parameterString = string.Join(", ", parameterNames);
            return $"{methodInfo.DeclaringType}.{methodInfo.Name}({parameterString})";
        }

        private static Dictionary<Type, string> typeMap = new Dictionary<Type, string>()
        {
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(bool), "bool"},
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(void), "void"},
            {typeof(char), "char"},
            {typeof(byte[]), "byte[]"},
            {typeof(sbyte[]), "sbyte[]"},
            {typeof(short[]), "short[]"},
            {typeof(ushort[]), "ushort[]"},
            {typeof(int[]), "int[]"},
            {typeof(uint[]), "uint[]"},
            {typeof(long[]), "long[]"},
            {typeof(ulong[]), "ulong[]"},
            {typeof(float[]), "float[]"},
            {typeof(double[]), "double[]"},
            {typeof(decimal[]), "decimal[]"},
            {typeof(bool[]), "bool[]"},
            {typeof(string[]), "string[]"},
            {typeof(char[]), "char[]"},
            {typeof(object[]), "object[]"},
        };

        public static string GetTypeName(this Type type)
        {
            if (typeMap.TryGetValue(type, out var name))
            {
                return name;
            }
            else
            {
                return type.FullName;
            }

            {
            }
        }
    }
}