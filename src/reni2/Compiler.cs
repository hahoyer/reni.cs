using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using System.Threading;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Runtime;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.UserInterface;
using Reni.Validation;

namespace Reni
{
    public sealed class Compiler : DumpableObject, IExecutionContext
    {
        static IScanner<SourceSyntax> Scanner(ITokenFactory<SourceSyntax> tokenFactory)
            => new Scanner<SourceSyntax>(Lexer.Instance, tokenFactory);

        readonly MainTokenFactory _tokenFactory;
        readonly CompilerParameters _parameters;
        readonly string _className;

        readonly ValueCache<Source> _source;
        readonly ValueCache<SourceSyntax> _sourceSyntax;
        readonly ValueCache<Syntax> _syntax;
        readonly ValueCache<CodeContainer> _codeContainer;
        readonly ValueCache<string> _cSharpCode;
        readonly FunctionCache<int, TokenInformation> _tokenCache;

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
        /// <param name="source"></param>
        public Compiler
            (
            string fileName = null,
            CompilerParameters parameters = null,
            string className = null,
            string text = null,
            Source source = null)
        {
            Tracer.Assert
                (
                    (fileName == null ? 0 : 1) +
                        (text == null ? 0 : 1) +
                        (source == null ? 0 : 1)
                        == 1
                );

            _className = className ?? fileName?.Symbolize() ?? "ReniMainClass";
            _rootContext = new Root(this);
            _parameters = parameters ?? new CompilerParameters();

            _tokenFactory = new MainTokenFactory(Scanner)
            {
                Trace = _parameters.TraceOptions.Parser
            };

            _source = new ValueCache<Source>
                (
                () => source
                    ?? (fileName == null
                        ? new Source(text)
                        : new Source(fileName.FileHandle()))
                );

            _tokenCache = new FunctionCache<int, TokenInformation>(GetTokenForCache);

            _sourceSyntax = new ValueCache<SourceSyntax>(() => Parse(Source + 0));

            _syntax = new ValueCache<Syntax>(() => SourceSyntax.Syntax);

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
        internal SourceSyntax SourceSyntax => _sourceSyntax.Value;

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

        Checked<CompileSyntax> IExecutionContext.Parse(string source) => Parse(source);

        Checked<CompileSyntax> Parse(string sourceText)
            => Parse(new Source(sourceText) + 0).Syntax.ToCompiledSyntax;

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

        [DisableDump]
        internal IEnumerable<Issue> Issues
            => SourceSyntax.Issues
                .plus(_parameters.ParseOnly ? new Issue[0] : CodeContainer.Issues)
            ;

        SourceSyntax Parse(SourcePosn source) => _tokenFactory.Parser.Execute(source);

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

        public TokenInformation Token(int offset) => _tokenCache[offset];

        TokenInformation GetTokenForCache(int offset) => Token(Source + offset);

        internal IEnumerable<SourceSyntax> FindAllBelongings(SourceSyntax current)
            => SourceSyntax.Belongings(current);

        public TokenInformation Token(SourcePosn posn)
        {
            var result = SourceSyntax.LocateToken(posn);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(posn);
            return null;
        }

        public TokenInformation Token(SourcePart part)
        {
            var left = SourceSyntax.LocateToken(part.Start);
            var right = SourceSyntax.LocateToken(part.End + -1);
            if(left.Equals(right))
                return left;

            var result = SourceSyntax.LocateToken(part);
            if(result != null)
                return new SyntaxToken(result);

            Tracer.TraceBreak();
            NotImplementedMethod(part);
            return null;
        }
    }

    public interface IOutStream
    {
        void AddData(string text);
        void AddLog(string text);
    }
}