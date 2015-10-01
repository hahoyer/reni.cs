using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using hw.Debug;
using hw.Helper;
using Microsoft.CSharp;
using Reni.Struct;

namespace Reni.Code
{
    static class Generator
    {
        static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();

        internal static string MainFunctionName => "MainFunction";

        internal static string FunctionName(FunctionId functionId)
            => (functionId.IsGetter ? "GetFunction" : "SetFunction") + functionId.Index;

        internal static string CreateCSharpString
            (
            Container main,
            FunctionCache<int, FunctionContainer> functions,
            bool useStatementAligner,
            string className
            )
            => new CSharp_Generated(className, main, functions).TransformText(useStatementAligner);

        internal static Assembly CreateCSharpAssembly
            (
            Container main,
            FunctionCache<int, FunctionContainer> functions,
            bool align,
            string className,
            bool traceFilePosn
            )
            => CodeToAssembly(CreateCSharpString(main, functions, align, className), traceFilePosn);

        static void CodeToFile(string name, string result, bool traceFilePosn)
        {
            var streamWriter = new StreamWriter(name);
            if(traceFilePosn)
                Tracer.Line
                    (Tracer.FilePosn(name.FileHandle().FullName, 0, 0, FilePositionTag.Debug));
            streamWriter.Write(result);
            streamWriter.Close();
        }

        static Assembly CodeToAssembly(string codeToString, bool traceFilePosn)
        {
            var name =
                Environment.GetEnvironmentVariable("temp")
                    + "\\reni.compiler\\"
                    + Thread.CurrentThread.ManagedThreadId
                    + ".reni.cs";
            var fileHandle = name.FileHandle();
            fileHandle.AssumeDirectoryOfFileExists();

            CodeToFile(name, codeToString, traceFilePosn);

            // Build the parameters for source compilation.
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateInMemory = true,
                CompilerOptions = "/unsafe /debug",
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(null, true)
            };
            var referencedAssemblies
                = new[]
                {
                    Assembly.GetAssembly(typeof(Generator)).Location,
                    Assembly.GetAssembly(typeof(hw.Helper.File)).Location
                };
            cp.ReferencedAssemblies.AddRange(referencedAssemblies);
            var cr = _provider.CompileAssemblyFromFile(cp, name);

            if(cr.Errors.Count > 0)
                HandleErrors(cr.Errors);

            return cr.CompiledAssembly;
        }

        static void HandleErrors(CompilerErrorCollection cr)
        {
            for(var i = 0; i < cr.Count; i++)
                Tracer.Line(cr[i].ToString());

            throw new CSharpCompilerErrorException(cr);
        }
    }

    sealed class CSharpCompilerErrorException : Exception
    {
        public CompilerErrorCollection CompilerErrorCollection { get; }

        public CSharpCompilerErrorException(CompilerErrorCollection cr)
        {
            CompilerErrorCollection = cr;
        }
    }

// ReSharper disable InconsistentNaming
    partial class CSharp_Generated
// ReSharper restore InconsistentNaming
    {
        readonly string _className;
        readonly Container _main;
        readonly FunctionCache<int, FunctionContainer> _functions;

        internal CSharp_Generated
            (string className, Container main, FunctionCache<int, FunctionContainer> functions)
        {
            _className = className;
            _main = main;
            _functions = functions;
        }

        internal string TransformText(bool useStatementAligner) => TransformText();
    }
}