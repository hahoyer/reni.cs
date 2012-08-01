#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Microsoft.CSharp;
using Reni.Struct;

namespace Reni.Code
{
    static class Generator
    {
        static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();

        internal static string MainFunctionName { get { return "MainFunction"; } }
        internal static string FunctionName(FunctionId functionId) { return (functionId.IsGetter ? "GetFunction" : "SetFunction") + functionId.Index; }

        internal static string CreateCSharpString(Container main, DictionaryEx<int, FunctionContainer> functions, bool useStatementAligner, string className) { return new CSharp_Generated(className, main, functions).TransformText(useStatementAligner); }
        internal static Assembly CreateCSharpAssembly(Container main, DictionaryEx<int, FunctionContainer> functions, bool align, string className, bool traceFilePosn) { return CodeToAssembly(CreateCSharpString(main, functions, align, className), traceFilePosn); }

        static void CodeToFile(string name, string result, bool traceFilePosn)
        {
            var streamWriter = new StreamWriter(name);
            if(traceFilePosn)
                Tracer.Line(Tracer.FilePosn(name.FileHandle().FullName, 0, 0, FilePositionTag.Debug));
            streamWriter.Write(result);
            streamWriter.Close();
        }

        static Assembly CodeToAssembly(string codeToString, bool traceFilePosn)
        {
            var name =
                Environment.GetEnvironmentVariable("temp")
                + "\\reni.compiler\\"
                + Thread.CurrentThread.ManagedThreadId
                + ".reni";
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
                    Assembly.GetAssembly(typeof(HWClassLibrary.IO.File)).Location,
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
        readonly CompilerErrorCollection _compilerErrorCollection;

        public CompilerErrorCollection CompilerErrorCollection { get { return _compilerErrorCollection; } }

        public CSharpCompilerErrorException(CompilerErrorCollection cr) { _compilerErrorCollection = cr; }
    }

// ReSharper disable InconsistentNaming
    partial class CSharp_Generated
// ReSharper restore InconsistentNaming
    {
        readonly string _className;
        readonly Container _main;
        readonly DictionaryEx<int, FunctionContainer> _functions;
        internal CSharp_Generated(string className, Container main, DictionaryEx<int, FunctionContainer> functions)
        {
            _className = className;
            _main = main;
            _functions = functions;
        }

        internal string TransformText(bool useStatementAligner) { return TransformText(); }
    }
}