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

        public static Compiler FromFile(string fileName, CompilerParameters parameters = null)
            => new Compiler(fileName, parameters);

        public static Compiler FromText(string text, CompilerParameters parameters = null, string id = null)
            => new Compiler(text: text, parameters: parameters, id:id);

        readonly MainTokenFactory _tokenFactory;
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

        Compiler
            (
            string fileName = null,
            CompilerParameters parameters = null,
            string moduleName = null,
            string text = null,
            
            string id = null)
        {
            Tracer.Assert
                (
                    new object[] {fileName, text}
                        .Count(item => item != null)
                        == 1
                );

            ModuleName = moduleName ?? ModuleNameFromFileName(fileName) ?? "ReniModule";
            Root = new Root(this);
            Parameters = parameters ?? new CompilerParameters();

            _tokenFactory = new MainTokenFactory(Scanner)
            {
                Trace = Parameters.TraceOptions.Parser
            };

            Source = (fileName == null
                    ? new Source(text,id)
                    : new Source(fileName.FileHandle()));

            CodeContainerCache = NewValueCache
                (() => new CodeContainer(ModuleName, Root, Syntax, Source.Data));
        }

        static string ModuleNameFromFileName(string fileName)
            => fileName == null ? null : "_" + Path.GetFileName(fileName).Symbolize();

        [Node]
        [DisableDump]
        internal Syntax Syntax => ValueCacheExtension.CachedValue(this,() => SourceSyntax.Syntax);

        [Node]
        [DisableDump]
        internal SourceSyntax SourceSyntax
        {
            get
            {
                lock(this)
                    return ValueCacheExtension.CachedValue(this, () => Parse(Source + 0));
            }
        }

        [Node]
        [DisableDump]
        public CodeContainer CodeContainer => CodeContainerCache.Value;

        [DisableDump]
        [Node]
        internal string CSharpString => ValueCacheExtension.CachedValue(this, () => CodeContainer.CSharpString);

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
            => _isInExecutionPhase && Parameters.TraceOptions.Functions;

        bool Root.IParent.ProcessErrors => Parameters.ProcessErrors;

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
            if(Parameters.TraceOptions.Source)
                Tracer.Line("Dump Source\n" + Source.Dump());

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

            foreach(var t in Issues)
                Parameters.OutStream.AddLog(t.LogDump + "\n");

            Data.OutStream = Parameters.OutStream;

            try
            {
                var method = CSharpString
                    .CodeToAssembly(Parameters.TraceOptions.GeneratorFilePosn)
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
            => SourceSyntax.Issues
                .plus(Parameters.ParseOnly ? new Issue[0] : CodeContainer.Issues)
            ;


        SourceSyntax Parse(SourcePosn source) => _tokenFactory.Parser.Execute(source);

        void RunFromCode() => CodeContainer.Execute(this, TraceCollector.Instance);

        internal void Materialize()
        {
            if(!Parameters.ParseOnly)
                CodeContainerCache.IsValid = true;
        }

        IOutStream IExecutionContext.OutStream => Parameters.OutStream;

        CodeBase IExecutionContext.Function(FunctionId functionId)
            => CodeContainer.Function(functionId);

        internal IEnumerable<SourceSyntax> FindAllBelongings(SourceSyntax sourceSyntax)
            => SourceSyntax.Belongings(sourceSyntax);

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