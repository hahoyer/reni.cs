using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Feature;
using Reni.Parser;

namespace Reni.SyntaxTree;

sealed class TerminalSyntax : ValueSyntax.NoChildren
{
    [Node]
    [DisableDump]
    internal readonly ITerminal Terminal;

    readonly SourcePart Token;

    [EnableDump]
    public string Id => Token.Id;

    [DisableDump]
    internal long ToNumber => BitsConst.Convert(Id).ToInt64();

    internal TerminalSyntax(ITerminal terminal, SourcePart token, Anchor anchor)
        : base(anchor)
    {
        Terminal = terminal;
        Token = token;
        StopByObjectIds();
        Token.AssertIsNotNull();
    }

    internal override Result GetResultForCache(ContextBase context, Category category)
        => Terminal.GetResult(context, category, Token);

    internal override ValueSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);
}

sealed class PrefixSyntax : ValueSyntax
{
    [Node]
    internal readonly ValueSyntax Right;

    [Node]
    readonly IPrefix Prefix;

    readonly SourcePart Token;

    public PrefixSyntax(IPrefix prefix, ValueSyntax right, SourcePart token, Anchor brackets)
        : base(brackets)
    {
        Prefix = prefix;
        Right = right;
        Token = token;
        Token.AssertIsNotNull();
    }

    protected override int DirectChildCount => 1;

    protected override Syntax GetDirectChild(int index) => index == 0? Right : null;

    internal override Result GetResultForCache(ContextBase context, Category category)
    {
        var (result, declaration) = Prefix.GetResult(context, category, Right, Token);
        Semantic.Declaration[context] = declaration;
        return result;
    }

    public static Result<ValueSyntax> Create
        (IPrefix prefix, Result<ValueSyntax> right, SourcePart token, Anchor brackets)
        => new PrefixSyntax(prefix, right.Target, token, brackets).AddIssues<ValueSyntax>(right.Issues);
}

sealed class InfixSyntax : ValueSyntax
{
    [Node]
    internal readonly ValueSyntax Left;

    [Node]
    internal readonly ValueSyntax Right;

    [Node]
    readonly IInfix Infix;

    [PublicAPI]
    readonly SourcePart Token;

    public InfixSyntax(ValueSyntax left, IInfix infix, ValueSyntax right, SourcePart token, Anchor brackets)
        : base(brackets)
    {
        Left = left;
        Infix = infix;
        Right = right;
        Token = token;
        Token.AssertIsNotNull();
        StopByObjectIds();
    }

    internal override IRecursionHandler RecursionHandler => Infix as IRecursionHandler;

    protected override int DirectChildCount => 2;

    protected override Syntax GetDirectChild(int index)
        => index switch
        {
            0 => Left, 1 => Right, _ => null
        };

    internal override Result GetResultForCache(ContextBase context, Category category) => Infix
        .GetResult(context, category, Left, Right);

    public static Result<ValueSyntax> Create
    (
        Result<ValueSyntax> left, IInfix infix, Result<ValueSyntax> right, SourcePart token
        , Anchor brackets
    )
    {
        ValueSyntax syntax = new InfixSyntax(left.Target, infix, right.Target, token, brackets);
        return syntax.AddIssues(left.Issues.Plus(right.Issues));
    }
}

interface IPendingProvider
{
    Result GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);
}

sealed class SuffixSyntax : ValueSyntax
{
    [Node]
    internal readonly ValueSyntax Left;

    [Node]
    readonly ISuffix Suffix;

    [PublicAPI]
    readonly SourcePart Token;

    internal SuffixSyntax(ValueSyntax left, ISuffix suffix, SourcePart token, Anchor brackets)
        : base(brackets)
    {
        Left = left;
        Suffix = suffix;
        Token = token;
        Token.AssertIsNotNull();
    }

    protected override int DirectChildCount => 1;

    protected override Syntax GetDirectChild(int index) => index == 0? Left : null;

    internal override Result GetResultForCache(ContextBase context, Category category)
        => Suffix.GetResult(context, category, Left);

    public static Result<ValueSyntax> Create
        (Result<ValueSyntax> left, ISuffix suffix, SourcePart token, Anchor brackets)
    {
        ValueSyntax syntax = new SuffixSyntax(left.Target, suffix, token, brackets);
        return syntax.AddIssues(left.Issues);
    }
}

interface ITerminal
{
    Result GetResult(ContextBase context, Category category, SourcePart token);
    ValueSyntax Visit(ISyntaxVisitor visitor);
    Declaration[] Declarations { get; }
}

interface IPrefix
{
    (Result, IImplementation) GetResult(ContextBase context, Category category, ValueSyntax right, SourcePart token);
}

interface IInfix
{
    Result GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);
}

interface ISuffix
{
    Result GetResult(ContextBase context, Category category, ValueSyntax left);
}