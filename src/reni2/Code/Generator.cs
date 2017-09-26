using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.CSharp;
using Reni.Struct;

namespace Reni.Code
{
    static class Generator
    {
        internal static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();

        internal static string MainFunctionName => "MainFunction";

        internal static string FunctionName(FunctionId functionId)
            => (functionId.IsGetter ? "GetFunction" : "SetFunction") + functionId.Index;

        internal static string CreateCSharpString
            (
            this string moduleName,
            Container main,
            FunctionCache<int, FunctionContainer> functions)
            => new CSharp_Generated(moduleName, main, functions).TransformText();

        internal static void CodeToFile(string name, string result, bool traceFilePosn)
        {
            var streamWriter = new StreamWriter(name);
            if(traceFilePosn)
                Tracer.Line
                    (Tracer.FilePosn(name.ToSmbFile().FullName, 0, 0, FilePositionTag.Debug));
            streamWriter.Write(result);
            streamWriter.Close();
        }

        internal static Assembly CodeToAssembly(this string codeToString, bool traceFilePosn, bool includeDebugInformation)
        {
            var name =
                Environment.GetEnvironmentVariable("temp")
                    + "\\reni.compiler\\"
                    + Process.GetCurrentProcess().Id
                    + "." + Thread.CurrentThread.ManagedThreadId
                    + ".reni.cs";
            name.ToSmbFile().EnsureDirectoryOfFileExists();

            CodeToFile(name, codeToString, traceFilePosn);

            // Build the parameters for source compilation.
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateInMemory = true,
                CompilerOptions = "/unsafe /debug",
                IncludeDebugInformation = includeDebugInformation,
                TempFiles = new TempFileCollection(null, false)
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
                                                                            
        internal static void HandleErrors(CompilerErrorCollection cr)
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
        readonly string ModuleName;
        readonly Container _main;
        readonly FunctionCache<int, FunctionContainer> _functions;

        internal CSharp_Generated
            (string moduleName, Container main, FunctionCache<int, FunctionContainer> functions)
        {
            ModuleName = moduleName;
            _main = main;
            _functions = functions;
        }
    }
}