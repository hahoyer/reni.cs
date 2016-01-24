using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Runtime;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;
using static hw.Helper.ValueCacheExtension;

namespace Reni
{
    public sealed class Compiler
        : DumpableObject, ValueCache.IContainer, Root.IParent, IExecutionContext
    {
        static IScanner<SourceSyntax> Scanner(ITokenFactory<SourceSyntax> tokenFactory)
            => new Scanner<SourceSyntax>(Lexer.Instance, tokenFactory);

        public static CompilerBrowser BrowserFromText
            (string text, CompilerParameters parameters = null)
            => new CompilerBrowser(new Compiler(text: text, parameters: parameters));

        public static Compiler FromFile(string fileName, CompilerParameters parameters)
            => new Compiler(fileName, parameters);

        internal static Compiler FromFile(string fileName) => new Compiler(fileName);
        public static Compiler FromText(string text) => new Compiler(text: text);

        readonly MainTokenFactory _tokenFactory;
        readonly CompilerParameters _parameters;
        readonly string ModuleName;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        [Node]
        [DisableDump]
        internal readonly Source Source;

        [Node]
        internal readonly Root Root;

        bool _isInExecutionPhase;

        [UsedImplicitly]
        public Exception Exception;
        readonly ValueCache<CodeContainer> CodeContainerCache;

        Compiler
            (
            string fileName = null,
            CompilerParameters parameters = null,
            string moduleName = null,
            string text = null,
            Source source = null)
        {
            Tracer.Assert
                (
                    new object[] {fileName, text, source}
                        .Count(item => item != null)
                        == 1
                );

            ModuleName = moduleName ?? ModuleNameFromFileName(fileName) ?? "ReniModule";
            Root = new Root(this);
            _parameters = parameters ?? new CompilerParameters();

            _tokenFactory = new MainTokenFactory(Scanner)
            {
                Trace = _parameters.TraceOptions.Parser
            };

            Source = source
                ?? (fileName == null
                    ? new Source(text)
                    : new Source(fileName.FileHandle()));

            CodeContainerCache = NewValueCache
                (() => new CodeContainer(ModuleName, Root, Syntax, Source.Data));
        }

        static string ModuleNameFromFileName(string fileName)
            => fileName == null ? null : "_" + Path.GetFileName(fileName).Symbolize();

        [Node]
        [DisableDump]
        internal Syntax Syntax => this.CachedValue(() => SourceSyntax.Syntax);

        [Node]
        [DisableDump]
        internal SourceSyntax SourceSyntax
        {
            get
            {
                lock(this)
                    return this.CachedValue(() => Parse(Source + 0));
            }
        }

        [Node]
        [DisableDump]
        public CodeContainer CodeContainer => CodeContainerCache.Value;

        [DisableDump]
        [Node]
        internal string CSharpString => this.CachedValue(() => CodeContainer.CSharpString);

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

        bool IsTraceEnabled
            => _isInExecutionPhase && _parameters.TraceOptions.Functions;

        bool Root.IParent.ProcessErrors => _parameters.ProcessErrors;

        IExecutionContext Root.IParent.ExecutionContext => this;

        Checked<CompileSyntax> Root.IParent.Parse(string source) => Parse(source);

        Checked<CompileSyntax> Parse(string sourceText)
            => Parse(new Source(sourceText) + 0).Syntax.ToCompiledSyntax;

        [UsedImplicitly]
        public Compiler Empower()
        {
            try
            {
                Execute();
            }
            catch(Exception exception)
            {
                Exception = exception;
            }
            return this;
        }

        /// <summary>
        ///     Performs compilation
        /// </summary>
        public void Execute()
        {
            if(_parameters.TraceOptions.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

            if(_parameters.TraceOptions.Syntax)
                Tracer.FlaggedLine("Syntax\n" + Syntax.Dump());

            if(_parameters.ParseOnly)
                return;

            if(_parameters.TraceOptions.CodeSequence)
                Tracer.FlaggedLine("Code\n" + CodeContainer.Dump());

            if(_parameters.RunFromCode)
            {
                _isInExecutionPhase = true;
                RunFromCode();
                _isInExecutionPhase = false;
                return;
            }

            if(_parameters.TraceOptions.ExecutedCode)
                Tracer.FlaggedLine(CSharpString);

            foreach(var t in Issues)
                _parameters.OutStream.AddLog(t.LogDump + "\n");

            Data.OutStream = _parameters.OutStream;
            try
            {
                var method = CSharpString
                    .CodeToAssembly(_parameters.TraceOptions.GeneratorFilePosn)
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

        internal void ExecuteFromCode(DataStack dataStack)
        {
            _isInExecutionPhase = true;
            CodeContainer.Main.Data.Visit(dataStack);
            _isInExecutionPhase = false;
        }

        [DisableDump]
        internal IEnumerable<Issue> Issues
            => SourceSyntax.Issues
                .plus(_parameters.ParseOnly ? new Issue[0] : CodeContainer.Issues)
            ;


        SourceSyntax Parse(SourcePosn source) => _tokenFactory.Parser.Execute(source);

        void RunFromCode() => CodeContainer.Execute(this, TraceCollector.Instance);

        internal void Materialize()
        {
            if(!_parameters.ParseOnly)
                CodeContainerCache.IsValid = true;
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
                c.Execute();
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

        IOutStream IExecutionContext.OutStream => _parameters.OutStream;

        CodeBase IExecutionContext.Function(FunctionId functionId)
            => CodeContainer.Function(functionId);
    }

    public sealed class TraceCollector : DumpableObject, ITraceCollector
    {
        internal static readonly ITraceCollector Instance = new TraceCollector();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
            => Tracer.Assert(false, dumper, depth + 1);

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            const string Stars = "\n******************************\n";
            Tracer.Line(Stars + dataStack.Dump() + Stars);
            Tracer.Line(codeBase.Dump());
            Tracer.IndentStart();
            codeBase.Visit(dataStack);
            Tracer.IndentEnd();
        }
    }

    public interface IOutStream
    {
        void AddData(string text);
        void AddLog(string text);
    }
}