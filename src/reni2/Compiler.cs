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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Runtime;
using Reni.Struct;

namespace Reni
{
    public sealed class Compiler : ReniObject, IExecutionContext
    {
        readonly string _fileName;
        readonly CompilerParameters _parameters;
        readonly ITokenFactory _tokenFactory = new MainTokenFactory();

        readonly SimpleCache<Source> _source;
        readonly SimpleCache<ReniParser.ParsedSyntax> _syntax;
        readonly SimpleCache<CodeBase> _code;
        readonly SimpleCache<CodeBasePair[]> _functionCode;
        readonly SimpleCache<Code.Container> _mainContainer;
        readonly SimpleCache<List<FunctionContainer>> _functionContainers;
        readonly SimpleCache<string> _executedCode;
        readonly SimpleCache<FunctionList> _functions;
        readonly SimpleCache<ContextBase> _rootContext;

        /// <summary>
        ///     ctor from file
        /// </summary>
        /// <param name="fileName"> Name of the file. </param>
        /// <param name="parameters"> </param>
        /// <param name="className"> </param>
        public Compiler(string fileName, CompilerParameters parameters = null, string className = null)
        {
            _fileName = fileName;
            _parameters = parameters ?? new CompilerParameters();
            _source = new SimpleCache<Source>(() => new Source(FileName.FileHandle()));
            _syntax = new SimpleCache<ReniParser.ParsedSyntax>(() => (ReniParser.ParsedSyntax) _tokenFactory.Parser.Compile(Source));
            _functionCode = new SimpleCache<CodeBasePair[]>(() => Functions.Code);
            _mainContainer = new SimpleCache<Code.Container>(() => new Code.Container(Code, Source.Data));
            _executedCode = new SimpleCache<string>(() => Generator.CreateCSharpString(MainContainer, FunctionContainers, true, className??fileName.Symbolize()));
            _functions = new SimpleCache<FunctionList>(() => new FunctionList());
            _functionContainers = new SimpleCache<List<FunctionContainer>>(() => Functions.Compile());
            _rootContext = new SimpleCache<ContextBase>(() => new Root(Functions, this));
            _code = new SimpleCache<CodeBase>(() => Struct.Container.Create(Syntax).Code(RootContext));
        }

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
        Code.Container MainContainer { get { return _mainContainer.Value; } }

        [DisableDump]
        List<FunctionContainer> FunctionContainers { get { return _functionContainers.Value; } }

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

        IOutStream IExecutionContext.OutStream { get { return _parameters.OutStream; } }
        bool IExecutionContext.IsTraceEnabled { get { return _parameters.Trace.Functions; } }

        CodeBase IExecutionContext.Function(FunctionId functionId) { throw new NotImplementedException(); }

        /// <summary>
        ///     Performs compilation
        /// </summary>
        public void Exec()
        {
            if(_parameters.Trace.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

            if(_parameters.Trace.Syntax)
                Tracer.FlaggedLine(FilePositionTag.Debug, "Syntax\n" + Syntax.Dump());

            if(_parameters.ParseOnly)
                return;

            if(_parameters.Trace.Functions)
            {
                Materialize();
                Tracer.FlaggedLine(FilePositionTag.Debug, "functions, Count = " + Functions.Count);
                for(var i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine(FilePositionTag.Debug, Functions[i].DumpFunction());
            }

            if(_parameters.Trace.CodeTree)
            {
                Tracer.FlaggedLine(FilePositionTag.Debug, "CodeTree");
                Tracer.FlaggedLine(FilePositionTag.Debug, "main\n" + Code.Dump());
                for(var i = 0; i < Functions.Count; i++)
                    Tracer.FlaggedLine(FilePositionTag.Debug, "function index=" + i + "\n" + Functions[i].BodyCode.Dump());
            }

            if(_parameters.RunFromCode)
            {
                RunFromCode();
                return;
            }

            if(_parameters.Trace.CodeSequence)
            {
                Tracer.FlaggedLine(FilePositionTag.Debug, "main\n" + MainContainer.Dump());
                for(var i = 0; i < FunctionContainers.Count; i++)
                    Tracer.FlaggedLine(FilePositionTag.Debug, "function index=" + i + "\n" + FunctionContainers[i].Dump());
            }
            if(_parameters.Trace.ExecutedCode)
                Tracer.FlaggedLine(FilePositionTag.Debug, ExecutedCode);

            Data.OutStream = _parameters.OutStream;
            try
            {
                var assembly = Generator.CreateCSharpAssembly(MainContainer, FunctionContainers, false, _parameters.Trace.GeneratorFilePosn,"Reni");
                var methodInfo = assembly.GetExportedTypes()[0].GetMethod(Generator.MainFunctionName);
                methodInfo.Invoke(null, new object[0]);
            }
            catch(CompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    _parameters.OutStream.Add(e.CompilerErrorCollection[i] + "\n");
            }
            Data.OutStream = null;
        }

        private void RunFromCode() { Code.Execute(this); }

        internal void Materialize()
        {
            if(_parameters.ParseOnly)
                return;
            _code.Ensure();
            for(var i = 0; i < Functions.Count; i++)
                Functions[i].EnsureBodyCode();
        }
    }

    public interface IOutStream
    {
        void Add(string text);
    }
}