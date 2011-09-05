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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Runtime;

namespace Reni
{
    public sealed class Compiler : ReniObject
    {
        readonly string _fileName;
        readonly CompilerParameters _parameters;
        readonly ITokenFactory _tokenFactory = new MainTokenFactory();

        readonly SimpleCache<Source> _source;
        readonly SimpleCache<ReniParser.ParsedSyntax> _syntax;
        readonly SimpleCache<CodeBase> _code;
        readonly SimpleCache<CodeBase[]> _functionCode;
        readonly SimpleCache<Container> _mainContainer;
        readonly SimpleCache<List<Container>> _functionContainers;
        readonly SimpleCache<string> _executedCode;
        readonly SimpleCache<FunctionList> _functions;
        readonly SimpleCache<ContextBase> _rootContext;

        /// <summary>
        ///     ctor from file
        /// </summary>
        /// <param name = "fileName">Name of the file.</param>
        /// <param name = "parameters"></param>
        internal Compiler(CompilerParameters parameters, string fileName)
        {
            _fileName = fileName;
            _parameters = parameters;
            _source = new SimpleCache<Source>(() => new Source(File.m(FileName)));
            _syntax = new SimpleCache<ReniParser.ParsedSyntax>(() => (ReniParser.ParsedSyntax) _tokenFactory.Parser.Compile(Source));
            _functionCode = new SimpleCache<CodeBase[]>(() => Functions.Code);
            _mainContainer = new SimpleCache<Container>(() => new Container(Code, Source.Data));
            _executedCode = new SimpleCache<string>(() => Generator.CreateCSharpString(MainContainer, FunctionContainers, true));
            _functions = new SimpleCache<FunctionList>(() => new FunctionList());
            _functionContainers = new SimpleCache<List<Container>>(() => Functions.Compile());
            _rootContext = new SimpleCache<ContextBase>(() => new Root(Functions));
            _code = new SimpleCache<CodeBase>(() => Struct.Container.Create(Syntax).Code(RootContext));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Compiler" /> class.
        /// </summary>
        /// <param name = "fileName">Name of the file.</param>
        /// created 14.07.2007 15:59 on HAHOYER-DELL by hh
        public Compiler(string fileName)
            : this(new CompilerParameters(), fileName) { }

        [Node]
        [DisableDump]
        internal FunctionList Functions { get { return _functions.Value; } }

        [DisableDump]
        string FileName { get { return _fileName; } }

        [DisableDump]
        Source Source { get { return _source.Value; } }

        [Node]
        [DisableDump]
        internal ReniParser.ParsedSyntax Syntax { get { return _syntax.Value; } }

        [DisableDump]
        internal string ExecutedCode { get { return _executedCode.Value; } }

        [Node]
        [DisableDump]
        CodeBase Code { get { return _code.Value; } }

        [DisableDump]
        Container MainContainer { get { return _mainContainer.Value; } }

        [DisableDump]
        List<Container> FunctionContainers { get { return _functionContainers.Value; } }

        [Node]
        [DisableDump]
        ContextBase RootContext { get { return _rootContext.Value; } }


        internal static string FormattedNow
        {
            get
            {
                var n = DateTime.Now;
                var result = "Date";
                result += n.Year.ToString("0000");
                result += n.Month.ToString("00");
                result += n.Day.ToString("00");
                result += "Time";
                result += n.Hour.ToString("00");
                result += n.Minute.ToString("00");
                result += n.Second.ToString("00");
                result += n.Millisecond.ToString("000");
                return result;
            }
        }
        public static OutStream OutStream { get { return BitsConst.OutStream; } set { BitsConst.OutStream = value; } }

        /// <summary>
        ///     Performs compilation
        /// </summary>
        internal OutStream Exec()
        {
            if(_parameters.Trace.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

            if(_parameters.Trace.Syntax)
                Tracer.FlaggedLine("Dump Syntax\n" + Syntax.Dump());

            if(_parameters.ParseOnly)
                return null;

            if(_parameters.Trace.Functions)
            {
                Materialize();
                Tracer.FlaggedLine("Dump functions, Count = " + Functions.Count);
                for(var i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine(Functions[i].DumpFunction());
            }

            if(_parameters.Trace.CodeTree)
            {
                Tracer.FlaggedLine("Dump CodeTree");
                Tracer.FlaggedLine("main\n" + Code.Dump());
                for(var i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine("function index=" + i + "\n" + Functions[i].BodyCode.Dump());
            }

            if(_parameters.RunFromCode)
                return GetOutStreamFromCode();

            if(_parameters.Trace.CodeSequence)
            {
                Tracer.FlaggedLine("main\n" + MainContainer.Dump());
                for(var i = 0; i < FunctionContainers.Count; i++)
                    Tracer.FlaggedLine("function index=" + i + "\n" + FunctionContainers[i].Dump());
            }
            if(_parameters.Trace.ExecutedCode)
                Tracer.FlaggedLine(ExecutedCode);

            return GetOutStream();
        }

        internal void Materialize()
        {
            if(_parameters.ParseOnly)
                return;
            _code.Ensure();
            for(var i = 0; i < Functions.Count; i++)
                Functions[i].EnsureBodyCode();
        }

        OutStream GetOutStream()
        {
            BitsConst.OutStream = new OutStream();
            try
            {
                var assembly = Generator.CreateCSharpAssembly(MainContainer, FunctionContainers, false);
                var methodInfo = assembly.GetExportedTypes()[0].GetMethod(Generator.MainFunctionName);
                methodInfo.Invoke(null, new object[0]);
            }
            catch(CompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    BitsConst.OutStream.Add(e.CompilerErrorCollection[i].ToString());
            }
            return BitsConst.OutStream;
        }

        OutStream GetOutStreamFromCode()
        {
            BitsConst.OutStream = new OutStream();
            Code.Execute(_functionCode.Value, _parameters.Trace.CodeExecutor);
            return BitsConst.OutStream;
        }
    }
}