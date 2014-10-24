using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Runtime;
using Reni.Struct;
using Reni.Syntax;
using Reni.Validation;

namespace Reni
{
    public sealed class Compiler : DumpableObject, IExecutionContext
    {
        readonly string _fileName;
        readonly CompilerParameters _parameters;
        readonly Root _rootContext;
        readonly string _className;
        static readonly ReniScanner _scanner = new ReniScanner();
        readonly MainTokenFactory _tokenFactory = new MainTokenFactory();

        readonly ValueCache<Source> _source;
        readonly ValueCache<ParsedSyntax> _syntax;
        readonly ValueCache<CodeContainer> _codeContainer;
        readonly ValueCache<string> _cSharpCode;

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

            _source = new ValueCache<Source>(() => new Source(FileName.FileHandle()));
            _syntax = new ValueCache<ParsedSyntax>(() => Parse(Source+0, _tokenFactory));
            _codeContainer = new ValueCache<CodeContainer>(() => new CodeContainer(RootContext, Syntax, Source.Data));
            _cSharpCode = new ValueCache<string>(() => _codeContainer.Value.CreateCSharpString(_className));
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

        [DisableDump]
        bool IsInExecutionPhase { get; set; }

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
        bool IExecutionContext.IsTraceEnabled { get { return IsInExecutionPhase && _parameters.Trace.Functions; } }
        CodeBase IExecutionContext.Function(FunctionId functionId) { return CodeContainer.Function(functionId); }
        CompileSyntax IExecutionContext.Parse(string source) { return Parse(source); }

        CompileSyntax Parse(string sourceText) { return Parse(new Source(sourceText) + 0, _tokenFactory).ToCompiledSyntax(); }

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
                var method = CodeContainer
                    .CreateCSharpAssembly(_className, _parameters.Trace.GeneratorFilePosn)
                    .GetExportedTypes()[0]
                    .GetMethod(Generator.MainFunctionName);

                IsInExecutionPhase = true;
                method.Invoke(null, new object[0]);
            }
            catch(CSharpCompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    _parameters.OutStream.AddLog(e.CompilerErrorCollection[i] + "\n");
            }
            finally
            {
                IsInExecutionPhase = false;
                Data.OutStream = null;
            }
        }

        internal IEnumerable<IssueBase> Issues { get { return CodeContainer.Issues; } }

        internal static ParsedSyntax Parse(SourcePosn source, ITokenFactory tokenFactory, Stack<OpenItem<IParsedSyntax>> stack = null)
        {
            return (ParsedSyntax) Position.Parse
                (
                    source,
                    tokenFactory,
                    _scanner,
                    stack);
        }

        void RunFromCode() { _codeContainer.Value.Execute(this); }

        internal void Materialize()
        {
            if(!_parameters.ParseOnly)
                _codeContainer.IsValid = true;
        }

        public static string FlatExecute(string text, bool isFakedName = false)
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
            {
                if(isFakedName)
                    log = log.Replace(fileName, "source");
                result += "Log: \n" + log + "\n";
            }

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