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
using System.Numerics;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public enum CompilationStatus
    {
        Compiled = 0,
        SomeCompiled = 1,
        NoneCompiled = 2,
        NoScripts = 3
    }
    public class CSharpScriptEngine
    {
        private readonly ILog _logger;
        private List<Assembly> _scriptAssembly = new List<Assembly>();
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        public CSharpScriptEngine()
        {
            _logger = LoggerProvider.GetLogger();
        }

        public CompilationStatus LoadSubdirectoryScripts(string folder)
        {
            var basePath = Path.GetFullPath(folder);
            var allfiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories).Where(pathString =>
            {
                var fileBasePath = Path.GetFullPath(pathString);
                var trimmedPath = fileBasePath.Remove(0, basePath.Length);
                var directories = trimmedPath.ToLower().Split(Path.DirectorySeparatorChar);
                if (directories.Contains("bin") || directories.Contains("obj"))
                {
                    return false;
                }
                if (pathString.Contains("AssemblyInfo.cs")) {
                    return false;
                }
                return true;
            });
            if (allfiles.Count() == 0)
            {
                return CompilationStatus.NoScripts;
            }
            return Load(new List<string>(allfiles));
        }

        /// <summary>
        /// Loads scripts from a list of files and compiles them.
        /// Takes about 300 milliseconds for a single script. Faster for a bunch of scripts.
        /// Returns an enum that defines the compilation state.
        /// </summary>
        /// <param name="scriptLocations"></param>
        /// <returns>Returns an enum that defines the compilation state.</returns>
        public CompilationStatus Load(List<string> scriptLocations)
        {
            var treeList = new SyntaxTree[scriptLocations.Count];
            Parallel.For(0, scriptLocations.Count, i =>
            {
                using (var sr = new StreamReader(scriptLocations[i]))
                {
                    // Read the stream to a string, and write the string to the console.
                    var syntaxTree = CSharpSyntaxTree.ParseText(sr.ReadToEnd(), null, scriptLocations[i]);
                    treeList[i] = syntaxTree;
                }
            });
            var assemblyName = Path.GetRandomFileName();

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !assembly.Location.Equals(""))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
            //Add vector2 reference because vector2 isn't in System.Numerics in dot net core
            references.Add(MetadataReference.CreateFromFile(typeof(Vector2).Assembly.Location));
            //Now add game reference
            references.Add(MetadataReference.CreateFromFile(typeof(Game).Assembly.Location));
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release).WithConcurrentBuild(true);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                treeList,
                references,
                compilationOptions
            );

            var state = CompilationStatus.Compiled;
            while (true)
            {
                using (var compiledAssemblies = new MemoryStream())
                {
                    var result = compilation.Emit(compiledAssemblies);

                    if (result.Success)
                    {
                        compiledAssemblies.Seek(0, SeekOrigin.Begin);
                        var assembly = Assembly.Load(compiledAssemblies.ToArray());
                        _scriptAssembly.Add(assembly);
                        foreach (var type in assembly.GetTypes())
                        {
                            types.Add(type.FullName, type);
                        }
                        return state;
                    }
                    else
                    {
                        state = CompilationStatus.SomeCompiled;
                        var invalidSourceTrees = GetInvalidSourceTrees(result);
                        compilation = compilation.RemoveSyntaxTrees(invalidSourceTrees);
                        if (invalidSourceTrees.Count == 0 || compilation.SyntaxTrees.Length == 0)
                        {
                            _logger.Error("Script compilation failed");
                            return CompilationStatus.NoneCompiled;
                        }
                    }
                }
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
            if (_scriptAssembly == null || _scriptAssembly.Count <= 0)
            {
                return default(T);
            }

            var fullClassName = scriptNamespace + "." + scriptClass;
            if (types.ContainsKey(fullClassName))
            {
                Type classType = (Type)types[fullClassName];
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
            if (_scriptAssembly == null || _scriptAssembly.Count <= 0)
            {
                return default(T);
            }

            scriptClass = scriptClass.Replace(" ", "_");
            var fullClassName = scriptNamespace + "." + scriptClass;

            if (types.ContainsKey(fullClassName))
            {
                Type classType = (Type)types[fullClassName];
                return (T)Activator.CreateInstance(classType);
            }

            _logger.Warn($"Could not find script: {scriptNamespace}.{scriptClass}");
            return default(T);
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