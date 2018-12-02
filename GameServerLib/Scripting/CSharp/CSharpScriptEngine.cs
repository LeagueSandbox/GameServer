using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using LeagueSandbox.GameServer.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

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

        public bool LoadSubdirectoryScripts(string folder)
        {
            var basePath = Path.GetFullPath(folder);
            var allfiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories).Where(pathString =>
            {
                var fileBasePath = Path.GetFullPath(pathString);
                var trimmedPath = fileBasePath.Remove(0, basePath.Length);
                var directories = trimmedPath.ToLower().Split(Path.DirectorySeparatorChar);
                if (directories.Contains("bin") || directories.Contains("obj")) return false;
                return true;
            });
            return !Load(new List<string>(allfiles));
        }

        //Takes about 300 milliseconds for a single script
        public bool Load(List<string> scriptLocations)
        {
            var treeList = new List<SyntaxTree>();
            Parallel.For(0, scriptLocations.Count, i =>
            {
                using (var sr = new StreamReader(scriptLocations[i]))
                {
                    // Read the stream to a string, and write the string to the console.
                    var syntaxTree = CSharpSyntaxTree.ParseText(sr.ReadToEnd(), null, scriptLocations[i]);
                    lock (treeList)
                    {
                        treeList.Add(syntaxTree);
                    }
                }
            });
            var assemblyName = Path.GetRandomFileName();

            var references = new List<MetadataReference>();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                if (!a.IsDynamic && !a.Location.Equals(""))
                    references.Add(MetadataReference.CreateFromFile(a.Location));
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

            var errored = false;
            while (true)
                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);

                    if (result.Success)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        _scriptAssembly = Assembly.Load(ms.ToArray());
                        return errored;
                    }

                    errored |= true;
                    var invalidSourceTrees = GetInvalidSourceTrees(result);

                    if (invalidSourceTrees.Count == 0)
                    {
                        // Shouldnt happen
                        _logger.Error("Script compilation failed");
                        return true;
                    }

                    compilation = compilation.RemoveSyntaxTrees(invalidSourceTrees);
                }
        }

        private List<SyntaxTree> GetInvalidSourceTrees(EmitResult result)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            return failures.Select(diagnostic =>
            {
                var loc = diagnostic.Location.SourceTree.GetLineSpan(diagnostic.Location.SourceSpan).Span;
                _logger.Error(
                    $"Script compilation error in script {diagnostic.Location.SourceTree.FilePath}: {diagnostic.Id}\n{diagnostic.GetMessage()} on " +
                    $"Line {loc.Start.Line} pos {loc.Start.Character} to Line {loc.End.Line} pos {loc.End.Character}");
                return diagnostic.Location.SourceTree;
            }).ToList();
        }

        public T GetStaticMethod<T>(string scriptNamespace, string scriptClass, string scriptFunction)
        {
            if (_scriptAssembly == null) return default(T);

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass, false);
            if (classType != null)
            {
                var desiredFunction = classType.GetMethod(scriptFunction, BindingFlags.Public | BindingFlags.Static);
                if (desiredFunction != null)
                    return (T) (object) Delegate.CreateDelegate(typeof(T), desiredFunction, false);
            }

            return default(T);
        }

        public T CreateObject<T>(string scriptNamespace, string scriptClass)
        {
            scriptClass = scriptClass.Replace(" ", "_");
            if (_scriptAssembly == null) return default(T);

            var classType = _scriptAssembly.GetType(scriptNamespace + "." + scriptClass);
            if (classType == null)
            {
                _logger.Warn($"Failed to load script: {scriptNamespace}.{scriptClass}");
                return default(T);
            }

            return (T) Activator.CreateInstance(classType);
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
            return (T) (object) Delegate.CreateDelegate(typeParameterType, obj, desiredFunction);
        }
    }
}