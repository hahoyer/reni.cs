using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reni.Runtime;
using Reni.Struct;

namespace Reni.Code
{
    static class Generator
    {
        internal static string MainFunctionName => "MainFunction";

        internal static string FunctionName(FunctionId functionId)
            => (functionId.IsGetter? "GetFunction" : "SetFunction") + functionId.Index;

        internal static string CreateCSharpString
        (
            this string moduleName,
            Container main,
            FunctionCache<int, FunctionContainer> functions
        )
            => new CSharp_Generated(moduleName, main, functions).TransformText();

        static void CodeToFile(string name, string result, bool traceFilePosition)
        {
            var streamWriter = new StreamWriter(name);
            if(traceFilePosition)
                Tracer.FilePosition(name.ToSmbFile().FullName, 0, 0, FilePositionTag.Debug).Log
                    ();
            streamWriter.Write(result);
            streamWriter.Close();
        }

        internal static Assembly CodeToAssembly
            (this string source, bool traceFilePosition, bool includeDebugInformation)
        {
            var name
                = Environment.GetEnvironmentVariable("temp") +
                "\\reni.compiler\\" +
                Process.GetCurrentProcess().Id +
                "." +
                Thread.CurrentThread.ManagedThreadId +
                ".reni.cs";
            CodeToFile(name, source, true);


            var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default, name, Encoding.Default
                , CancellationToken.None);

            var assemblyName = Path.GetRandomFileName();
            var references = T(
                    Path.GetDirectoryName(typeof(GCSettings).GetTypeInfo().Assembly.Location).PathCombine("System.Runtime.dll"),
                    Assembly.GetAssembly(typeof(Data)).Location,
                    Assembly.GetAssembly(typeof(object)).Location
                )
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToArray();

            var compilation = CSharpCompilation
                .Create(
                    assemblyName,
                    new[] {syntaxTree},
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if(!result.Success)
                HandleErrors(result.Diagnostics);

            ms.Seek(0, SeekOrigin.Begin);
            return AssemblyLoadContext.Default.LoadFromStream(ms);
        }

        static void HandleErrors(ImmutableArray<Diagnostic> errors)
        {
            var errorList = errors
                .Select(error => error.ToString())
                .ToArray();
            errorList.Stringify("\n").Log();
            throw new CSharpCompilerErrorException(errorList);
        }

        static TValue[] T<TValue>(params TValue[] value) => value;
    }

    sealed class CSharpCompilerErrorException : Exception
    {
        public string[] Errors { get; }
        public CSharpCompilerErrorException(string[] errors) => Errors = errors;
    }

// ReSharper disable InconsistentNaming
    partial class CSharp_Generated
// ReSharper restore InconsistentNaming
    {
        readonly FunctionCache<int, FunctionContainer> Functions;
        readonly Container Main;
        readonly string ModuleName;

        internal CSharp_Generated(string moduleName, Container main, FunctionCache<int, FunctionContainer> functions)
        {
            ModuleName = moduleName;
            Main = main;
            Functions = functions;
        }
    }
}