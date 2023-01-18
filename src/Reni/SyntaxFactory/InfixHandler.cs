using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory;

sealed class InfixHandler : DumpableObject, IValueProvider
{
    abstract class InfixTypeErrorTokenClass : DumpableObject, IIssueTokenClass
    {
        readonly IssueId IssueId;
        readonly ITokenClass ActualTokenClass;


        string Message
        {
            get
            {
                var tokenClass = ActualTokenClass;
                var types = new List<string>();
                if(tokenClass is IInfix)
                    types.Add("infix");
                if(tokenClass is ISuffix)
                    types.Add("suffix");
                if(tokenClass is IPrefix)
                    types.Add("prefix");
                if(tokenClass is ITerminal)
                    types.Add("terminal");

                return $"(actual: {types.Stringify(",")})";
            }
        }
        IEnumerable<string> Types
        {
            get
            {
                var tokenClass = ActualTokenClass;
                var types = new List<string>();
                if(tokenClass is IInfix)
                    types.Add("infix");
                if(tokenClass is ISuffix)
                    types.Add("suffix");
                if(tokenClass is IPrefix)
                    types.Add("prefix");
                if(tokenClass is ITerminal)
                    types.Add("terminal");

                return types;
            }
        }

        protected InfixTypeErrorTokenClass(IssueId issueId, ITokenClass actualTokenClass)
        {
            IssueId = issueId;
            ActualTokenClass = actualTokenClass;
        }

        IssueId IIssueTokenClass.IssueId => IssueId;
        string ITokenClass.Id => $"<error:{IssueId}/{ActualTokenClass}>";

        protected Issue GetIssue(SourcePart sourcePart) => IssueId.GetIssue(sourcePart, Types.Stringify("/"));
    }

    sealed class InfixErrorTokenClass : InfixTypeErrorTokenClass, IInfix
    {
        public InfixErrorTokenClass(ITokenClass tokenClass)
            : base(IssueId.InvalidInfixExpression, tokenClass) { }

        Result IInfix.Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => new(category, GetIssue(left.Anchor.SourcePart + right.Anchor.SourcePart));
    }

    sealed class SuffixErrorTokenClass : InfixTypeErrorTokenClass, ISuffix
    {
        public SuffixErrorTokenClass(ITokenClass tokenClass)
            : base(IssueId.InvalidSuffixExpression, tokenClass) { }

        Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
            => new(category, GetIssue(left.Anchor.SourcePart));
    }

    sealed class PrefixErrorTokenClass : InfixTypeErrorTokenClass, IPrefix
    {
        public PrefixErrorTokenClass(ITokenClass tokenClass)
            : base(IssueId.InvalidPrefixExpression, tokenClass) { }

        Result IPrefix.Result(ContextBase context, Category category, ValueSyntax right, SourcePart token)
            => new(category, GetIssue(right.Anchor.SourcePart));
    }

    sealed class TerminalErrorTokenClass : InfixTypeErrorTokenClass, ITerminal
    {
        static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };

        public TerminalErrorTokenClass(ITokenClass tokenClass)
            : base(IssueId.InvalidTerminalExpression, tokenClass) { }

        Declaration[] ITerminal.Declarations => PredefinedDeclarations;


        Result ITerminal.Result(ContextBase context, Category category, SourcePart token)
            => new(category, GetIssue(token));

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }
    }

    static readonly FunctionCache<System.Type, FunctionCache<ITokenClass, InfixTypeErrorTokenClass>>
        From
            = new(type => new(tokenClass
                => GetErrorTokenClass(type, tokenClass)));

    ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor anchor)
    {
        var left = factory.GetValueSyntax(target.Left);
        var right = factory.GetValueSyntax(target.Right);

        var tokenClass = GetTokenClass(left != null, target.TokenClass, right != null);

        return left
            .GetInfixSyntax(tokenClass, target.Token, right, Anchor.Create(target).Combine(anchor));
    }

    static ITokenClass GetTokenClass(bool left, ITokenClass tokenClass, bool right)
        => left? right
                ? GetTokenClass<IInfix>(tokenClass)
                : GetTokenClass<ISuffix>(tokenClass) :
            right? GetTokenClass<IPrefix>(tokenClass) : GetTokenClass<ITerminal>(tokenClass);

    static ITokenClass GetTokenClass<TInfixType>(ITokenClass tokenClass)
        => tokenClass is TInfixType? tokenClass : GetErrorTokenClass<TInfixType>(tokenClass);

    static ITokenClass GetErrorTokenClass<TInfixType>(ITokenClass tokenClass)
        => From[typeof(TInfixType)][tokenClass];

    static InfixTypeErrorTokenClass GetErrorTokenClass(System.Type type, ITokenClass tokenClass)
    {
        if(type.Is<IInfix>())
            return new InfixErrorTokenClass(tokenClass);
        if(type.Is<ISuffix>())
            return new SuffixErrorTokenClass(tokenClass);
        if(type.Is<IPrefix>())
            return new PrefixErrorTokenClass(tokenClass);
        if(type.Is<ITerminal>())
            return new TerminalErrorTokenClass(tokenClass);
        throw new InvalidOperationException();
    }
}