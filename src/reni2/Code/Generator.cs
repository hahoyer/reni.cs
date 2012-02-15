//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Microsoft.CSharp;
using Reni.Basics;
using Reni.Runtime;

namespace Reni.Code
{
    static class Generator
    {
        static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();
        static readonly string[] _referencedAssemblies = new[] {"reni.dll", "HWClassLibrary.dll"};
        internal const string FrameArgName = "frame";

        public static string NotACommentFlag { get { return "<notacomment> "; } }
        internal static string MainFunctionName { get { return "MainFunction"; } }
        internal static string FunctionName(int i) { return "Function" + i; }

        internal static string CreateCSharpString(Container main, List<Container> functions, bool useStatementAligner) { return new CSharp_Generated(main, functions).TransformText(useStatementAligner); }
        internal static Assembly CreateCSharpAssembly(Container main, List<Container> functions, bool align, bool traceFilePosn) { return CodeToAssembly(CreateCSharpString(main, functions, align),traceFilePosn); }

        static void CodeToFile(string name, string result, bool traceFilePosn)
        {
            var streamWriter = new StreamWriter(name);
            if(traceFilePosn)
                Tracer.Line(Tracer.FilePosn(name.FileHandle().FullName, 0, 0, ""));
            streamWriter.Write(result);
            streamWriter.Close();
        }

        static Assembly CodeToAssembly(string codeToString, bool traceFilePosn)
        {
            const string name = "generated.cs";
            CodeToFile(name, codeToString, traceFilePosn);

            // Build the parameters for source compilation.
            var cp = new System.CodeDom.Compiler.CompilerParameters
                     {
                         GenerateInMemory = true,
                         CompilerOptions = "/unsafe /debug",
                         IncludeDebugInformation = true,
                         TempFiles = new TempFileCollection(null, true)
                     };
            cp.ReferencedAssemblies.AddRange(_referencedAssemblies);
            var cr = _provider.CompileAssemblyFromFile(cp, name);

            if(cr.Errors.Count > 0)
                HandleErrors(cr.Errors);

            return cr.CompiledAssembly;
        }

        static void HandleErrors(CompilerErrorCollection cr)
        {
            for(var i = 0; i < cr.Count; i++)
                Tracer.Line(cr[i].ToString());

            throw new CompilerErrorException(cr);
        }
    }

    sealed class CompilerErrorException : Exception
    {
        readonly CompilerErrorCollection _compilerErrorCollection;

        public CompilerErrorCollection CompilerErrorCollection { get { return _compilerErrorCollection; } }

        public CompilerErrorException(CompilerErrorCollection cr) { _compilerErrorCollection = cr; }
    }

// ReSharper disable InconsistentNaming
    partial class CSharp_Generated
// ReSharper restore InconsistentNaming
    {
        readonly Container _main;
        readonly List<Container> _functions;
        internal CSharp_Generated(Container main, List<Container> functions)
        {
            _main = main;
            _functions = functions;
        }

        static int RefBytes { get { return DataHandler.RefBytes; } }

        internal string TransformText(bool useStatementAligner)
        {
            var result = TransformText();
            return result;
        }
    }
}