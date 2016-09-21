using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
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
        const string DefaultSourceIdentifier = "source";
        const string DefaultModuleName = "ReniModule";

        static IScanner<Syntax> Scanner(ITokenFactory<Syntax> tokenFactory)
            => new Scanner<Syntax>(Lexer.Instance, tokenFactory);

        public static Compiler FromFile(string fileName, CompilerParameters parameters = null)
        {
            Tracer.Assert(fileName != null);
            var moduleName = ModuleNameFromFileName(fileName);
            return new Compiler(new Source(fileName.FileHandle()), moduleName, parameters);
        }

        public static Compiler FromText
            (string text, CompilerParameters parameters = null, string sourceIdentifier = null)
        {
            Tracer.Assert(text != null);
            return new Compiler
                (
                new Source(text, sourceIdentifier ?? DefaultSourceIdentifier),
                DefaultModuleName,
                parameters);
        }

        readonly MainTokenFactory TokenFactory;
        internal readonly CompilerParameters Parameters;
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

        Compiler(Source source, string modulName, CompilerParameters parameters)
        {
            Source = source;
            Parameters = parameters ?? new CompilerParameters();
            ModuleName = modulName;

            Root = new Root(this);
            TokenFactory = new MainTokenFactory(Scanner)
            {
                Trace = Parameters.TraceOptions.Parser
            };

            CodeContainerCache = NewValueCache
                (() => new CodeContainer(ModuleName, Root, Syntax, Source.Data));
        }

        static string ModuleNameFromFileName(string fileName)
            => "_" + Path.GetFileName(fileName).Symbolize();

        [Node]
        [DisableDump]
        public Syntax Syntax
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

        bool IsTraceEnabled
            => _isInExecutionPhase && Parameters.TraceOptions.Functions;

        bool Root.IParent.ProcessErrors => Parameters.ProcessErrors;

        IExecutionContext Root.IParent.ExecutionContext => this;

        IEnumerable<ScannerTokenClass> Root.IParent.AllTokenClasses
            => TokenFactory.AllTokenClasses;

        Result<Value> Root.IParent.Parse(string source) => Parse(source);

        Result<Value> Parse(string sourceText)
            => Parse(new Source(sourceText) + 0).Value;

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
            if(Parameters.TraceOptions.Syntax)
                Tracer.FlaggedLine("Syntax\n" + Syntax.Dump());

            if(Parameters.ParseOnly)
                return;

            if(Parameters.TraceOptions.CodeSequence)
                Tracer.FlaggedLine("Code\n" + CodeContainer.Dump());

            if(Parameters.RunFromCode)
            {
                _isInExecutionPhase = true;
                RunFromCode();
                _isInExecutionPhase = false;
                return;
            }

            if(Parameters.TraceOptions.ExecutedCode)
                Tracer.FlaggedLine(CSharpString);

            if (Parameters.OutStream != null)
                foreach(var t in Issues)
                    Parameters.OutStream.AddLog(t.LogDump + "\n");

            Data.OutStream = Parameters.OutStream;

            try
            {
                var method = CSharpString
                    .CodeToAssembly(Parameters.TraceOptions.GeneratorFilePosn, Debugger.IsAttached)
                    .GetExportedTypes()[0]
                    .GetMethod(Generator.MainFunctionName);

                _isInExecutionPhase = true;
                method.Invoke(null, new object[0]);
            }
            catch(CSharpCompilerErrorException e)
            {
                for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                    Parameters.OutStream.AddLog(e.CompilerErrorCollection[i] + "\n");
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
            => Syntax.AllIssues
                .plus(Parameters.ParseOnly ? new Issue[0] : CodeContainer.Issues)
            ;


        Syntax Parse(SourcePosn source) => TokenFactory.Parser.Execute(source);

        void RunFromCode() => CodeContainer.Execute(this, TraceCollector.Instance);

        internal void Materialize()
        {
            if(!Parameters.ParseOnly)
                CodeContainerCache.IsValid = true;
        }

        IOutStream IExecutionContext.OutStream => Parameters.OutStream;

        CodeBase IExecutionContext.Function(FunctionId functionId)
            => CodeContainer.Function(functionId);

        internal IEnumerable<Syntax> FindAllBelongings(Syntax syntax)
            => Syntax.Belongings(syntax);

        public static string FlatExecute(string code, bool b) => "";
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

        void ITraceCollector.Call(StackData argsAndRefs, FunctionId functionId)
        {
            Tracer.Line("\n>>>>>> Call" + functionId.NodeDump + "\n");
            Tracer.IndentStart();
        }

        void ITraceCollector.Return()
        {
            Tracer.IndentEnd();
            Tracer.Line("\n<<<<<< Return\n");
        }
    }

    public interface IOutStream
    {
        void AddData(string text);
        void AddLog(string text);
    }
}