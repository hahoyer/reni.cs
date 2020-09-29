using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        internal static Assembly CodeToAssembly
            (this string codeToString, bool traceFilePosition, bool includeDebugInformation)
        {
            var directoryName
                = Environment.GetEnvironmentVariable
                      ("temp") +
                  "\\reni.compiler\\" +
                  Process.GetCurrentProcess().Id +
                  "." +
                  Thread.CurrentThread.ManagedThreadId;
            directoryName.ToSmbFile().EnsureIsExistentDirectory();
            //nameof(directoryName).IsSetTo(directoryName).WriteLine();
            var name = directoryName + ".reni.cs";
            name.ToSmbFile().CheckedEnsureDirectoryOfFileExists();

            CodeToFile(name, codeToString, traceFilePosition);

            // Build the parameters for source compilation.
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateInMemory = true,
                CompilerOptions = "/unsafe",
                IncludeDebugInformation = includeDebugInformation,
                TempFiles = new TempFileCollection(directoryName, false)
            };
            var referencedAssemblies
                = T
                (
                    Assembly.GetAssembly(typeof(Generator)).Location,
                    Assembly.GetAssembly(typeof(SmbFile)).Location
                );
            cp.ReferencedAssemblies.AddRange(referencedAssemblies);
            var cr = _provider.CompileAssemblyFromFile(cp, name);

            if(!includeDebugInformation)
                directoryName.ToSmbFile().Delete(true);

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

        static TValue[] T<TValue>(params TValue[] value) => value;
    }

    sealed class CSharpCompilerErrorException : Exception
    {
        public CSharpCompilerErrorException(CompilerErrorCollection cr) => CompilerErrorCollection = cr;

        public CompilerErrorCollection CompilerErrorCollection {get;}
    }

// ReSharper disable InconsistentNaming
    partial class CSharp_Generated
// ReSharper restore InconsistentNaming
    {
        readonly FunctionCache<int, FunctionContainer> _functions;
        readonly Container _main;
        readonly string ModuleName;

        internal CSharp_Generated
            (string moduleName, Container main, FunctionCache<int, FunctionContainer> functions)
        {
            ModuleName = moduleName;
            _main = main;
            _functions = functions;
        }
    }
}