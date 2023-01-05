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
using Reni.Helper;
using Reni.Numeric;
using Reni.Parser;
using Reni.Runtime;
using Reni.Struct;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni;

public sealed class Compiler
    : Compiler<BinaryTree>, ValueCache.IContainer, Root.IParent, IExecutionContext
{
    internal const string PredefinedSource = "?Predefined";
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

    readonly ValueCache<BinaryTree> BinaryTreeCache;
    readonly ValueCache<CodeContainer> CodeContainerCache;

    readonly MainTokenFactory MainTokenFactory;
    readonly string ModuleName;
    readonly ValueCache<ValueSyntax> ValueSyntaxCache;

    bool IsInExecutionPhase;

    [Node]
    [DisableDump]
    internal BinaryTree BinaryTree => BinaryTreeCache.Value;

    [Node]
    [DisableDump]
    internal BinaryTree BinaryTreeWithSyntaxLink
    {
        get
        {
            ValueSyntaxCache.IsValid = true;
            return BinaryTree;
        }
    }

    [Node]
    [DisableDump]
    internal ValueSyntax Syntax
    {
        get
        {
            var result = ValueSyntaxCache.Value;
            AssertValidSyntaxLinkForBinaryTree();
            return result;
        }
    }

    [Node]
    [DisableDump]
    internal CodeContainer CodeContainer => Parameters.IsCodeRequired? CodeContainerCache.Value : null;

    [DisableDump]
    [Node]
    internal string CSharpString => this.CachedValue(() => CodeContainer?.CSharpString);

    [UsedImplicitly]
    bool IsTraceEnabled
        => IsInExecutionPhase && Parameters.TraceOptions.Functions;

    MethodInfo CSharpMethod
    {
        get
        {
            try
            {
                var includeDebugInformation = Parameters.DebuggableGeneratedCode ?? Debugger.IsAttached;
                return CSharpString?
                    .CodeToAssembly
                        (Parameters.TraceOptions.GeneratorFilePosition, includeDebugInformation)
                    .GetExportedTypes()[0]
                    .GetMethod(Generator.MainFunctionName);
            }
            catch(CSharpCompilerErrorException e)
            {
                foreach(var error in e.Errors)
                    Parameters.OutStream.AddLog(error + "\n");

                return null;
            }
        }
    }

    [DisableDump]
    internal IEnumerable<Issue> Issues
    {
        get
        {
            var binaryTree = BinaryTree.AllIssues.ToArray();
            var syntax = ValueSyntaxCache.Value?.AllIssues.ToArray() ?? new Issue[0];
            var code = CodeContainer?.Issues.ToArray() ?? new Issue[0];
            //$"binaryTree: {binaryTree.Length}, syntax: {syntax.Length}, code: {code.Length}".Log(FilePositionTag.Debug);
            return T(binaryTree, syntax, code).ConcatMany();
        }
    }

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
            (
                EqualityOperation.TokenId(false)
                , EqualityOperation.TokenId(true)
                , IdentityOperation.TokenId(false)
                , IdentityOperation.TokenId(true)
            );

            result += PrioTable.Left(NotOperation.TokenId);
            result += PrioTable.Left("&");
            result += PrioTable.Left("|");


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
                    LeftParenthesis.TokenId(3)
                    , LeftParenthesis.TokenId(2)
                    , LeftParenthesis.TokenId(1)
                    , PrioTable.BeginOfText
                },
                new[]
                {
                    RightParenthesisBase.TokenId(3)
                    , RightParenthesisBase.TokenId(2)
                    , RightParenthesisBase.TokenId(1)
                    , PrioTable.EndOfText
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
                    LeftParenthesis.TokenId(3)
                    , LeftParenthesis.TokenId(2)
                    , LeftParenthesis.TokenId(1)
                },
                new[]
                {
                    RightParenthesisBase.TokenId(3)
                    , RightParenthesisBase.TokenId(2)
                    , RightParenthesisBase.TokenId(1)
                }
            );
            result.Title = "Declaration";
            return result;
        }
    }

    [DisableDump]
    internal TypeBase MainType => this.CachedValue(() => Syntax.Type(Root));

    Compiler(Source source, string moduleName, CompilerParameters parameters)
    {
        (source != null).Assert();
        Source = source;
        Parameters = parameters ?? new CompilerParameters();
        ModuleName = moduleName;

        var main = this["Main"];
        var declaration = this["Declaration"];

        MainTokenFactory = new(declaration, "Main");

        main.PrioTable = MainPriorityTable;
        main.TokenFactory = new ScannerTokenFactory();
        main.Add<ScannerTokenType<BinaryTree>>(MainTokenFactory);

        declaration.PrioTable = DeclarationPriorityTable;
        declaration.TokenFactory = new ScannerTokenFactory(true);
        declaration.BoxFunction = target => new ExclamationBoxToken(target);
        declaration.Add<ScannerTokenType<BinaryTree>>(new DeclarationTokenFactory("Declaration"));

        main.Parser.Trace = Parameters.TraceOptions.Parser;
        declaration.Parser.Trace = Parameters.TraceOptions.Parser;

        //Tracer.FlaggedLine(PrettyDump);

        Root = new(this);
        CodeContainerCache = new(GetCodeContainer);
        BinaryTreeCache = new(() => Parse(Source));
        ValueSyntaxCache = new(GetSyntax);
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

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
        (fileName != null).Assert();
        var moduleName = ModuleNameFromFileName(fileName);
        return new(new(fileName.ToSmbFile()), moduleName, parameters);
    }

    public static Compiler FromText
        (string text, CompilerParameters parameters = null, string sourceIdentifier = null)
    {
        (text != null).Assert();
        return new(
            new(text, sourceIdentifier ?? DefaultSourceIdentifier),
            DefaultModuleName,
            parameters);
    }

    ValueSyntax GetSyntax() => Parameters.IsSyntaxRequired? GetSyntax(BinaryTree) : null;

    static ValueSyntax GetSyntax(BinaryTree target) => Factory.Root.GetFrameSyntax(target);

    void AssertValidSyntaxLinkForBinaryTree()
    {
        var anchor = BinaryTree;
        (anchor.TokenClass is EndOfText).Assert();
        foreach(var item in anchor.GetNodesFromLeftToRight())
            item.Syntax.AssertIsNotNull(() => item.LogDump());
    }

    CodeContainer GetCodeContainer() => new(Syntax, Root, ModuleName, Source.Data);

    static string ModuleNameFromFileName(string fileName)
        => "_" + Path.GetFileName(fileName).Symbolize();

    Result<ValueSyntax> ParsePredefinedItem(string sourceText)
        => Factory.Root.GetValueSyntax(Parse(new(sourceText, PredefinedSource)).BracketKernel.Center);

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

        if(Parameters.TraceOptions.CodeSequence && Parameters.IsCodeRequired)
            ("Code\n" + CodeContainer.Dump()).FlaggedLine();

        if(Parameters.RunFromCode)
        {
            IsInExecutionPhase = true;
            RunFromCode();
            IsInExecutionPhase = false;
            return;
        }

        if(Parameters.TraceOptions.ExecutedCode)
            ("ExecutedCode:\n" + CSharpString).FlaggedLine();

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

    BinaryTree Parse(Source source) => this["Main"].Parser.Execute(source);

    void RunFromCode() => CodeContainer.Execute(this, TraceCollector.Instance);

    internal void Materialize()
    {
        if(Parameters.IsCodeRequired)
            CodeContainerCache.IsValid = true;
    }
}

public sealed class TraceCollector : DumpableObject, ITraceCollector
{
    internal static readonly ITraceCollector Instance = new TraceCollector();

    void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
        => false.Assert(dumper, depth + 1);

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