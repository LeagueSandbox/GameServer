using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class CSharpScriptEngine
    {
        private readonly ILog _logger;
        private Assembly _scriptAssembly;

        public CSharpScriptEngine()
        {
            _logger = LoggerProvider.GetLogger();
        }

        //Takes about 300 milliseconds for a single script
        public bool LoadFromData(Dictionary<string, byte[]> scriptFiles)
        {
            bool compiledSuccessfully;
            var treeList = new List<SyntaxTree>();
            Parallel.ForEach(scriptFiles, keyValuePair =>
            {
                _logger.Debug($"Loading script: {keyValuePair.Key}");
                using (var sr = new StreamReader(new MemoryStream(keyValuePair.Value)))
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
                if (!a.IsDynamic && !a.Location.Equals(""))
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
                        _logger.Error($"{diagnostic.Id}: {diagnostic.GetMessage()} with location: {loc.SourceTree}");
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

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass, false);
            if (classType != null)
            {
                var desiredFunction = classType.GetMethod(scriptFunction, BindingFlags.Public | BindingFlags.Static);
                if (desiredFunction != null)
                {
                    return (T)(object)Delegate.CreateDelegate(typeof(T), desiredFunction, false);
                }
            }

            return default(T);
        }

        public T CreateObject<T>(string scriptNamespace, string scriptClass)
        {
            scriptClass = scriptClass.Replace(" ", "_");
            _logger.Debug("Loading game script for: " + scriptNamespace + ", " + scriptClass);
            if (_scriptAssembly == null)
            {
                return default(T);
            }

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass);
            if (classType == null)
            {
                _logger.Warn($"Failed to load script: {scriptNamespace}.{scriptClass}");
                return default(T);
            }
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
