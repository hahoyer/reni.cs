using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Context;
using Reni.Numeric;
using Reni.Parser;
using Reni.Runtime;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;
using static hw.Helper.ValueCacheExtension;

namespace Reni
{
    public sealed class Compiler
        : Compiler<BinaryTree>, ValueCache.IContainer, Root.IParent, IExecutionContext
    {
        const string DefaultSourceIdentifier = "source";
        const string DefaultModuleName = "ReniModule";

        [UsedImplicitly]
        public Exception Exception;

        internal readonly CompilerParameters Parameters;

        [Node]
        internal readonly Root Root;

        [Node]
        [DisableDump]
        internal readonly Source Source;

        bool IsInExecutionPhase;
        readonly ValueCache<BinaryTree> BinaryTreeCache;
        readonly ValueCache<CodeContainer> CodeContainerCache;

        readonly MainTokenFactory MainTokenFactory;
        readonly string ModuleName;
        readonly ValueCache<Result<ValueSyntax>> ValueSyntaxCache;

        Compiler(Source source, string moduleName, CompilerParameters parameters)
        {
            Tracer.Assert(source != null);
            Source = source;
            Parameters = parameters ?? new CompilerParameters();
            ModuleName = moduleName;

            var main = this["Main"];
            var declaration = this["Declaration"];

            MainTokenFactory = new MainTokenFactory(declaration, "Main");

            main.PrioTable = MainPriorityTable;
            main.TokenFactory = new ScannerTokenFactory();
            main.Add<ScannerTokenType<BinaryTree>>(MainTokenFactory);

            declaration.PrioTable = DeclarationPriorityTable;
            declaration.TokenFactory = new ScannerTokenFactory();
            declaration.BoxFunction = target => new ExclamationBoxToken(target);
            declaration.Add<ScannerTokenType<BinaryTree>>(new DeclarationTokenFactory("Declaration"));

            main.Parser.Trace = Parameters.TraceOptions.Parser;
            declaration.Parser.Trace = Parameters.TraceOptions.Parser;

            //Tracer.FlaggedLine(PrettyDump);

            Root = new Root(this);
            CodeContainerCache = NewValueCache(GetCodeContainer);
            BinaryTreeCache = NewValueCache(() => Parse(Source + 0));
            ValueSyntaxCache = NewValueCache(GetValueSyntax);
        }

        [Node]
        [DisableDump]
        public BinaryTree BinaryTree => BinaryTreeCache.Value;

        [Node]
        [DisableDump]
        internal ValueSyntax Syntax => ValueSyntaxCache.Value.Target;

        [Node]
        [DisableDump]
        public CodeContainer CodeContainer => Parameters.IsCodeRequired? CodeContainerCache.Value : null;

        [DisableDump]
        [Node]
        internal string CSharpString => this.CachedValue(() => CodeContainer.CSharpString);

        bool IsTraceEnabled
            => IsInExecutionPhase && Parameters.TraceOptions.Functions;

        MethodInfo CSharpMethod
        {
            get
            {
                try
                {
                    var includeDebugInformation = Parameters.DebuggableGeneratedCode ?? Debugger.IsAttached;
                    return CSharpString
                        .CodeToAssembly
                            (Parameters.TraceOptions.GeneratorFilePosition, includeDebugInformation)
                        .GetExportedTypes()[0]
                        .GetMethod(Generator.MainFunctionName);
                }
                catch(CSharpCompilerErrorException e)
                {
                    for(var i = 0; i < e.CompilerErrorCollection.Count; i++)
                        Parameters.OutStream.AddLog(e.CompilerErrorCollection[i] + "\n");

                    return null;
                }
            }
        }

        [DisableDump]
        internal IEnumerable<Issue> Issues
            => T(BinaryTree.Issues, ValueSyntaxCache.Value?.Issues, CodeContainer?.Issues).Concat();

        static PrioTable MainPriorityTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);
                result += PrioTable.Left
                (
                    AtToken.TokenId,
                    "_N_E_X_T_",
                    ToNumberOfBase.TokenId
                );

                result += PrioTable.Left(ConcatArrays.TokenId, ConcatArrays.MutableId);

                result += PrioTable.Left(Star.TokenId, Slash.TokenId, "\\");
                result += PrioTable.Left(Plus.TokenId, Minus.TokenId);

                result += PrioTable.Left
                (
                    CompareOperation.TokenId(),
                    CompareOperation.TokenId(canBeEqual: true),
                    CompareOperation.TokenId(false),
                    CompareOperation.TokenId(false, true)
                );
                result += PrioTable.Left
                    (EqualityOperation.TokenId(false), EqualityOperation.TokenId());

                result += PrioTable.Right(ReassignToken.TokenId);

                result += PrioTable.Right(ThenToken.TokenId);
                result += PrioTable.Right(ElseToken.TokenId);

                result += PrioTable.Right(Exclamation.TokenId);
                result += PrioTable.Left
                (
                    TokenClasses.Function.TokenId(),
                    TokenClasses.Function.TokenId(true),
                    TokenClasses.Function.TokenId(isMetaFunction: true));
                result += PrioTable.Right(Colon.TokenId);
                result += PrioTable.Right(TokenClasses.List.TokenId(0));
                result += PrioTable.Right(TokenClasses.List.TokenId(1));
                result += PrioTable.Right(TokenClasses.List.TokenId(2));
                result += PrioTable.Right(Cleanup.TokenId);
                result += PrioTable.Right(PrioTable.Error);

                result += PrioTable.BracketParallels
                (
                    new[]
                    {
                        LeftParenthesis.TokenId(3), LeftParenthesis.TokenId(2), LeftParenthesis.TokenId(1)
                        , PrioTable.BeginOfText
                    },
                    new[]
                    {
                        RightParenthesisBase.TokenId(3), RightParenthesisBase.TokenId(2)
                        , RightParenthesisBase.TokenId(1), PrioTable.EndOfText
                    }
                );

                result.Title = "Main";
                //Tracer.FlaggedLine("\n"+x.ToString());
                return result;
            }
        }

        static PrioTable DeclarationPriorityTable
        {
            get
            {
                var result = PrioTable.Left(PrioTable.Any);
                result += PrioTable.BracketParallels
                (
                    new[]
                    {
                        LeftParenthesis.TokenId(3), LeftParenthesis.TokenId(2), LeftParenthesis.TokenId(1)
                        , PrioTable.BeginOfText
                    },
                    new[]
                    {
                        RightParenthesisBase.TokenId(3), RightParenthesisBase.TokenId(2)
                        , RightParenthesisBase.TokenId(1), PrioTable.EndOfText
                    }
                );
                result.Title = "Declaration";
                return result;
            }
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        CodeBase IExecutionContext.Function(FunctionId functionId)
            => CodeContainer.Function(functionId);

        IOutStream IExecutionContext.OutStream => Parameters.OutStream;

        IEnumerable<Definable> Root.IParent.DefinedNames
            => MainTokenFactory.AllTokenClasses.OfType<Definable>();

        IExecutionContext Root.IParent.ExecutionContext => this;

        Result<ValueSyntax> Root.IParent.ParsePredefinedItem(string source) => ParsePredefinedItem(source);

        bool Root.IParent.ProcessErrors => Parameters.ProcessErrors;

        public static Compiler FromFile(string fileName, CompilerParameters parameters = null)
        {
            Tracer.Assert(fileName != null);
            var moduleName = ModuleNameFromFileName(fileName);
            return new Compiler(new Source(fileName.ToSmbFile()), moduleName, parameters);
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

        Result<ValueSyntax> GetValueSyntax()
            => Parameters.IsSyntaxRequired? SyntaxFactory.Root.GetValueSyntax(BinaryTree).ToFrame() : null;

        CodeContainer GetCodeContainer() => new CodeContainer(Syntax, Root, ModuleName, Source.Data);

        static string ModuleNameFromFileName(string fileName)
            => "_" + Path.GetFileName(fileName).Symbolize();

        Result<ValueSyntax> ParsePredefinedItem(string sourceText)
        {
            var syntax = Parse(new Source(sourceText) + 0);

            Tracer.Assert(syntax.Left != null);
            Tracer.Assert(syntax.TokenClass is EndOfText);
            Tracer.Assert(syntax.Right == null);

            Tracer.Assert(syntax.Left.Left == null);
            Tracer.Assert(syntax.Left.TokenClass is BeginOfText);

            return syntax.Left.Right.Syntax(null);
        }

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
            if(!Parameters.IsSyntaxRequired)
            {
                BinaryTreeCache.IsValid = true;
                return;
            }

            if(Parameters.TraceOptions.CodeSequence)
                Tracer.FlaggedLine("Code\n" + CodeContainer.Dump());

            if(Parameters.RunFromCode)
            {
                IsInExecutionPhase = true;
                RunFromCode();
                IsInExecutionPhase = false;
                return;
            }

            if(Parameters.TraceOptions.ExecutedCode)
                Tracer.FlaggedLine("ExecutedCode:\n" + CSharpString);

            if(Parameters.OutStream != null)
                foreach(var t in Issues)
                    Parameters.OutStream.AddLog(t.LogDump + "\n");

            var method = CSharpMethod;

            if(method == null)
                return;
            if(!Parameters.IsRunRequired)
                return;

            Data.OutStream = Parameters.OutStream;
            IsInExecutionPhase = true;
            method.Invoke(null, new object[0]);
            IsInExecutionPhase = false;
            Data.OutStream = null;
        }

        internal void ExecuteFromCode(DataStack dataStack)
        {
            IsInExecutionPhase = true;
            CodeContainer.Main.Data.Visit(dataStack);
            IsInExecutionPhase = false;
        }

        BinaryTree Parse(SourcePosition source) => this["Main"].Parser.Execute(source);

        void RunFromCode() => CodeContainer.Execute(this, TraceCollector.Instance);

        internal void Materialize()
        {
            if(Parameters.IsCodeRequired)
                CodeContainerCache.IsValid = true;
        }

        internal IEnumerable<BinaryTree> FindAllBelongings(BinaryTree binaryTree)
            => BinaryTree.Belongings(binaryTree);
    }

    public sealed class TraceCollector : DumpableObject, ITraceCollector
    {
        internal static readonly ITraceCollector Instance = new TraceCollector();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
            => Tracer.Assert(false, dumper, depth + 1);

        void ITraceCollector.Call(StackData argsAndRefs, FunctionId functionId)
        {
            ("\n>>>>>> Call" + functionId.NodeDump + "\n").Log();
            Tracer.IndentStart();
        }

        void ITraceCollector.Return()
        {
            Tracer.IndentEnd();
            "\n<<<<<< Return\n".Log();
        }

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            const string stars = "\n******************************\n";
            (stars + dataStack.Dump() + stars).Log();
            codeBase.Dump().Log();
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