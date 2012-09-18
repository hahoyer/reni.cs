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
using System.Threading;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Runtime;
using Reni.Struct;
using Reni.Validation;

namespace Reni
{
    public sealed class Compiler : ReniObject, IExecutionContext
    {
        readonly string _fileName;
        readonly CompilerParameters _parameters;
        readonly ITokenFactory _tokenFactory = new MainTokenFactory();
        readonly Root _rootContext;
        readonly string _className;

        readonly SimpleCache<Source> _source;
        readonly SimpleCache<ParsedSyntax> _syntax;
        readonly SimpleCache<CodeContainer> _codeContainer;
        readonly SimpleCache<string> _cSharpCode;

        /// <summary>
        ///     ctor from file
        /// </summary>
        /// <param name="fileName"> Name of the file. </param>
        /// <param name="parameters"> </param>
        /// <param name="className"> </param>
        public Compiler(string fileName, CompilerParameters parameters = null, string className = null)
        {
            _className = className ?? fileName.Symbolize();
            _fileName = fileName;
            _rootContext = new Root(this);
            _parameters = parameters ?? new CompilerParameters();
        
            _source = new SimpleCache<Source>(() => new Source(FileName.FileHandle()));
            _syntax = new SimpleCache<ParsedSyntax>(() => (ParsedSyntax) _tokenFactory.Parser.Compile(Source));
            _codeContainer = new SimpleCache<CodeContainer>(() => new CodeContainer(RootContext, Syntax, Source.Data));
            _cSharpCode = new SimpleCache<string>(() => _codeContainer.Value.CreateCSharpString(_className));
        }

        [DisableDump]
        string FileName { get { return _fileName; } }
        [Node]
        [DisableDump]
        Source Source { get { return _source.Value; } }
        [Node]
        [DisableDump]
        internal ParsedSyntax Syntax { get { return _syntax.Value; } }
        [Node]
        [DisableDump]
        CodeContainer CodeContainer { get { return _codeContainer.Value; } }
        [DisableDump]
        [Node]
        internal string CSharpCode { get { return _cSharpCode.Value; } }
        [Node]
        [DisableDump]
        Root RootContext { get { return _rootContext; } }


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
        CodeBase IExecutionContext.Function(FunctionId functionId) { return CodeContainer.Function(functionId); }

        /// <summary>
        ///     Performs compilation
        /// </summary>
        public void Exececute()
        {
            if(_parameters.Trace.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

            if(_parameters.Trace.Syntax)
                Tracer.FlaggedLine("Syntax\n" + Syntax.Dump());

            if(_parameters.ParseOnly)
                return;

            if(_parameters.RunFromCode)
            {
                RunFromCode();
                return;
            }

            if(_parameters.Trace.CodeSequence)
                Tracer.FlaggedLine("Code\n" + CodeContainer.Dump());

            if(_parameters.Trace.ExecutedCode)
                Tracer.FlaggedLine(CSharpCode);

            foreach(var t in Issues)
                _parameters.OutStream.AddLog(t.LogDump + "\n");

            Data.OutStream = _parameters.OutStream;
            try
            {
                CodeContainer
                    .CreateCSharpAssembly(_className, _parameters.Trace.GeneratorFilePosn)
                    .GetExportedTypes()[0]
                    .GetMethod(Generator.MainFunctionName)
                    .Invoke(null, new object[0]);
            }
            catch(CSharpCompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    _parameters.OutStream.AddLog(e.CompilerErrorCollection[i] + "\n");
            }
            Data.OutStream = null;
        }

        internal IEnumerable<IssueBase> Issues { get { return CodeContainer.Issues; } }

        void RunFromCode() { _codeContainer.Value.Execute(this); }

        internal void Materialize()
        {
            if(!_parameters.ParseOnly)
                _codeContainer.Ensure();
        }
        
        public static string FlatExecute(string text)
        {
            var fileName =
                Environment.GetEnvironmentVariable("temp")
                + "\\reni.server\\"
                + Thread.CurrentThread.ManagedThreadId
                + ".reni";
            var fileHandle = fileName.FileHandle();
            fileHandle.AssumeDirectoryOfFileExists();
            fileHandle.String = text;
            var stringStream = new StringStream();
            var parameters = new CompilerParameters {OutStream = stringStream};
            var c = new Compiler(fileName, parameters);

            var exceptionText = "";
            try
            {
                c.Exececute();
            }
            catch(Exception exception)
            {
                exceptionText = exception.Message;
            }

            var result = "";

            var log = stringStream.Log;
            if(log != "")
                result += "Log: \n" + log + "\n";

            var data = stringStream.Data;
            if(data != "")
                result += "Data: \n" + data + "\n";

            if(exceptionText != "")
                result += "Exception: \n" + exceptionText;

            return result;
        }
    }

    public interface IOutStream
    {
        void AddData(string text);
        void AddLog(string text);
    }
}