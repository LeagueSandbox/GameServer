﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public class CSharpScriptEngine
    {
        private Assembly _scriptAssembly;

        private Logger _logger = Program.ResolveDependency<Logger>();

        public bool LoadSubdirectoryScripts(string folder)
        {
            var allfiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
            return Load(new List<string>(allfiles));
        }

        //Takes about 300 milliseconds for a single script
        public bool Load(List<string> scriptLocations)
        {
            var compiledSuccessfully = true;
            var treeList = new List<SyntaxTree>();
            Parallel.For(0, scriptLocations.Count, i => {
                _logger.LogCoreInfo($"Loading script: {scriptLocations[i]}");
                using (var sr = new StreamReader(scriptLocations[i]))
                {
                    // Read the stream to a string, and write the string to the console.
                    var syntaxTree = CSharpSyntaxTree.ParseText(sr.ReadToEnd());
                    lock (treeList)
                    {
                        treeList.Add(syntaxTree);
                    }
                }
            });
            var assemblyName = Path.GetRandomFileName();
            
            var references = new List<MetadataReference>();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!a.IsDynamic && a.Location != "")
                {
                    references.Add(MetadataReference.CreateFromFile(a.Location));
                }
            }
            //Now add game reference
            references.Add(MetadataReference.CreateFromFile(typeof(Game).Assembly.Location));
            var op = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release).WithConcurrentBuild(true);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                treeList,
                references,
                op
            );

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    compiledSuccessfully = false;
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        var loc = diagnostic.Location;
                        _logger.LogCoreError($"{diagnostic.Id}: {diagnostic.GetMessage()} with location: {loc.SourceTree}");
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    _scriptAssembly = Assembly.Load(ms.ToArray());
                    compiledSuccessfully = true;
                }
            }

            return compiledSuccessfully;
        }

        public T GetStaticMethod<T>(string scriptNamespace, string scriptClass, string scriptFunction)
        {
            if (_scriptAssembly == null)
            {
                return default(T);
            }

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass);
            var desiredFunction = classType.GetMethod(scriptFunction, BindingFlags.Public | BindingFlags.Static);

            var typeParameterType = typeof(T);
            return (T)(object)Delegate.CreateDelegate(typeParameterType, desiredFunction);
        }

        public T CreateObject<T>(string scriptNamespace, string scriptClass)
        {
            if (_scriptAssembly == null)
            {
                return default(T);
            }

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass);

            return (T)Activator.CreateInstance(classType);
        }
        public static object RunFunctionOnObject(object obj, string method, params object[] args)
        {
            return obj.GetType().InvokeMember(
                method,
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                obj,
                args
            );
        }
        public static T GetObjectMethod<T>(object obj, string scriptFunction)
        {
            var classType = obj.GetType();
            var desiredFunction = classType.GetMethod(scriptFunction, BindingFlags.Public | BindingFlags.Instance);

            var typeParameterType = typeof(T);
            return (T)(object)Delegate.CreateDelegate(typeParameterType, obj, desiredFunction);
        }
    }
}
