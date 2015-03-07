using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using System.Threading;
using hw.Debug;
using hw.Forms;
using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Runtime;
using Reni.Struct;
using Reni.Validation;

namespace Reni
{
    public sealed class Compiler : DumpableObject, IExecutionContext
    {
        readonly MainTokenFactory _tokenFactory;
        readonly CompilerParameters _parameters;
        readonly string _className;

        readonly ValueCache<Source> _source;
        readonly ValueCache<Syntax> _syntax;
        readonly ValueCache<CodeContainer> _codeContainer;
        readonly ValueCache<string> _cSharpCode;

        [Node]
        readonly Root _rootContext;

        bool _isInExecutionPhase;
        /// <summary>
        ///     ctor from file
        /// </summary>
        /// <param name="fileName"> Name of the file. </param>
        /// <param name="parameters"> </param>
        /// <param name="className"> </param>
        /// <param name="text"></param>
        public Compiler
            (
            string fileName = null,
            CompilerParameters parameters = null,
            string className = null,
            string text = null)
        {
            Tracer.Assert((fileName == null) != (text == null));

            _className = className ?? fileName?.Symbolize() ?? "ReniMainClass";
            _rootContext = new Root(this);
            _parameters = parameters ?? new CompilerParameters();
            _tokenFactory = new MainTokenFactory
            {
                Trace = _parameters.TraceOptions.Parser
            };

            _source = new ValueCache<Source>
                (() => fileName == null ? new Source(text) : new Source(fileName.FileHandle()));
            _tokenCache = new FunctionCache<int, Token>(GetTokenForCache);
            _syntax = new ValueCache<Syntax>(() => Parse(Source + 0));
            _codeContainer = new ValueCache<CodeContainer>
                (() => new CodeContainer(_rootContext, Syntax, Source.Data));
            _cSharpCode = new ValueCache<string>
                (() => _codeContainer.Value.CreateCSharpString(_className));
        }


        [Node]
        [DisableDump]
        public Source Source => _source.Value;

        [Node]
        [DisableDump]
        internal Syntax Syntax => _syntax.Value;

        [Node]
        [DisableDump]
        CodeContainer CodeContainer => _codeContainer.Value;

        [DisableDump]
        [Node]
        internal string CSharpCode => _cSharpCode.Value;

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

        IOutStream IExecutionContext.OutStream => _parameters.OutStream;
        bool IExecutionContext.IsTraceEnabled
            => _isInExecutionPhase && _parameters.TraceOptions.Functions;
        bool IExecutionContext.ProcessErrors => _parameters.ProcessErrors;
        CodeBase IExecutionContext.Function(FunctionId functionId)
            => CodeContainer.Function(functionId);
        CompileSyntax IExecutionContext.Parse(string source) => Parse(source);

        CompileSyntax Parse(string sourceText) => Parse(new Source(sourceText) + 0).ToCompiledSyntax;

        /// <summary>
        ///     Performs compilation
        /// </summary>
        public void Exececute()
        {
            if(_parameters.TraceOptions.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

            if(_parameters.TraceOptions.Syntax)
                Tracer.FlaggedLine("Syntax\n" + Syntax.Dump());

            if(_parameters.ParseOnly)
                return;

            if(_parameters.RunFromCode)
            {
                RunFromCode();
                return;
            }

            if(_parameters.TraceOptions.CodeSequence)
                Tracer.FlaggedLine("Code\n" + CodeContainer.Dump());

            if(_parameters.TraceOptions.ExecutedCode)
                Tracer.FlaggedLine(CSharpCode);

            foreach(var t in Issues)
                _parameters.OutStream.AddLog(t.LogDump + "\n");

            Data.OutStream = _parameters.OutStream;
            try
            {
                var method = CodeContainer
                    .CreateCSharpAssembly(_className, _parameters.TraceOptions.GeneratorFilePosn)
                    .GetExportedTypes()[0]
                    .GetMethod(Generator.MainFunctionName);

                _isInExecutionPhase = true;
                method.Invoke(null, new object[0]);
            }
            catch(CSharpCompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    _parameters.OutStream.AddLog(e.CompilerErrorCollection[i] + "\n");
            }
            finally
            {
                _isInExecutionPhase = false;
                Data.OutStream = null;
            }
        }

        internal IEnumerable<IssueBase> Issues => CodeContainer.Issues;

        Syntax Parse(SourcePosn source) => _tokenFactory.Parser.Execute(source);

        void RunFromCode() => _codeContainer.Value.Execute(this);

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
            var parameters = new CompilerParameters
            {
                OutStream = stringStream
            };
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

        readonly FunctionCache<int, Token> _tokenCache;

        public Token Token(int offset) => _tokenCache[offset];

        Token GetTokenForCache(int offset)
        {
            var sourcePosn = Source + offset;
            var result = Syntax.LocateToken(sourcePosn);
            if(result != null)
                return result;

            var kind = SpecialToken.Type.WhiteSpace;
            var l = ReniLexer.LexerForUserInterfaceInstance.PlainWhiteSpace(sourcePosn);
            if(l == null)
            {
                kind = SpecialToken.Type.LineComment;
                l = ReniLexer.LexerForUserInterfaceInstance.LineComment(sourcePosn);
            }
            if(l == null)
            {
                kind = SpecialToken.Type.Comment;
                l = ReniLexer.LexerForUserInterfaceInstance.Comment(sourcePosn);
            }

            if(l == null)
            {
                kind = SpecialToken.Type.Error;
                l = 1;
            }

            return new SpecialToken(SourcePart.Span(sourcePosn, l.Value), kind);
        }
    }


    public interface IOutStream
    {
        void AddData(string text);
        void AddLog(string text);
    }
}