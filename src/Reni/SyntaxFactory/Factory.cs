using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory;

sealed class Factory : DumpableObject
{
    internal static readonly IValueProvider Bracket = new BracketHandler();
    internal static readonly IValueProvider MatchedBracket = new MatchedBracketHandler();
    internal static readonly IValueProvider Definable = new DefinableHandler();
    internal static readonly IValueProvider ScannerError = new InfixHandler();

    internal static readonly IValueProvider Infix = new InfixHandler();
    internal static readonly IValueProvider Function = new FunctionHandler();
    internal static readonly IValueProvider Cleanup = new CleanupHandler();
    internal static readonly IValueProvider Else = new ElseHandler();
    internal static readonly IValueProvider Then = new ThenHandler();

    internal static readonly IStatementsProvider List = new ListHandler();
    internal static readonly IStatementProvider Colon = new ColonHandler();

    internal static readonly IValueProvider Annotations = new AnnotationHandler();


    internal static readonly Factory Root = new(false);

    [EnableDump]
    internal readonly bool MeansPublic;


    Factory(bool meansPublic) => MeansPublic = meansPublic;

    internal ValueSyntax GetFrameSyntax(BinaryTree target)
    {
        var kernel = target.BracketKernel;
        var statements = GetStatementsSyntax(kernel.Center, null, kernel.Center?.TokenClass);
        var listAnchors = kernel.Center?.ParserLevelGroup;
        if(listAnchors != null && listAnchors.Any() && listAnchors[0].TokenClass is not TokenClasses.List)
            listAnchors = null;
        var anchor = kernel.ToAnchor.Combine(listAnchors);
        return CompoundSyntax.Create(statements, null, anchor);
    }

    internal IStatementSyntax[] GetStatementsSyntax(BinaryTree target, Anchor anchor, ITokenClass master)
    {
        if(target == null)
        {
            (anchor == null || anchor.IsEmpty).Assert();
            return new IStatementSyntax[0];
        }

        var factory = GetCurrentFactory(target);

        switch(target.TokenClass)
        {
            case IDeclarationToken declarationToken:
                // ReSharper disable once ConvertClosureToMethodGroup
                anchor.AssertIsNull();
                return T(declarationToken.Provider.Get(target, factory));
            case IStatementsToken statementsToken when target.TokenClass.IsBelongingTo(master):
                return statementsToken.Provider.Get(target, factory, anchor);
            default:
                return T((IStatementSyntax)GetValueSyntax(target, anchor));
        }
    }

    internal ValueSyntax GetValueSyntax(BinaryTree target, Anchor anchor)
    {
        if(target == null)
            return new EmptyList(anchor);

        var factory = GetCurrentFactory(target);

        switch(target.TokenClass)
        {
            case ExclamationBoxToken:
                var (item, annotations) = target.CheckForAnnotations();
                return GetAnnotatedValueSyntax(GetValueSyntax(item, anchor), annotations);
            case IValueToken valueToken:
                return valueToken.Provider.Get(target, factory, anchor);
            case IDeclarationToken declarationToken:
                return declarationToken.Provider.Get(target, factory).ToValueSyntax(anchor);
            case IStatementsToken statementsToken:
                return factory.GetStatementsSyntax(target, anchor, statementsToken);
            case IIssueTokenClass issue:
                return GetIssueSyntax(issue.IssueId, target, anchor);

            default:
                return new EmptyList
                (
                    Anchor.CreateAll(target).Combine(anchor, true)
                    , IssueId.InvalidExpression.GetIssue(target.Token)
                );
        }
    }

    static ValueSyntax GetAnnotatedValueSyntax
        (ValueSyntax target, (BinaryTree annotation, BinaryTree[] anchors)[] annotations)
    {
        var result = target;
        foreach(var (annotation, anchors) in annotations)
            result = AnnotationSyntax.Create
            (
                result
                , (IValueAnnotation)annotation.TokenClass
                , anchors.Concat(T(annotation)).ToArray()
            );
        return result;
    }

    ValueSyntax GetStatementsSyntax(BinaryTree target, Anchor anchor, IStatementsToken tokenClass)
    {
        var node = tokenClass
            .Provider
            .Get(target, this);
        anchor = Anchor.Create(target.ParserLevelGroup).Combine(anchor, true);
        return CompoundSyntax.Create(node, null, anchor);
    }

    ValueSyntax GetIssueSyntax(IssueId issueId, BinaryTree target, Anchor anchor)
    {
        var resultingAnchor = Anchor.Create(target).Combine(anchor);

        if(issueId == IssueId.ExtraLeftBracket)
        {
            target.Left.AssertIsNull();
            return GetValueSyntax(target.Right, resultingAnchor);
        }

        if(issueId == IssueId.ExtraRightBracket)
        {
            target.Right.AssertIsNull();
            return GetValueSyntax(target.Left, resultingAnchor);
        }

        NotImplementedMethod(issueId, target, anchor);
        return new EmptyList(Anchor.CreateAll(target).Combine(anchor, true));
    }

    internal ValueSyntax GetValueSyntax(BinaryTree target)
        => target == null? null : GetValueSyntax(target, Anchor.Create("Should be a value"));

    Factory GetCurrentFactory(BinaryTree target)
    {
        var level = target.GetBracketLevel();
        if(level == null)
            return this;

        return level switch
            {
                0 => true, 3 => true, _ => false
            }
            == MeansPublic
                ? this
                : new(!MeansPublic);
    }

    internal ExpressionSyntax GetExpressionSyntax(BinaryTree target, Anchor anchor)
    {
        var definable = target.TokenClass as Definable;
        (definable != null).Assert();
        return ExpressionSyntax
            .Create
            (
                GetValueSyntax(target.Left)
                , definable
                , target.Token
                , GetValueSyntax(target.Right)
                , Anchor.Create(target).Combine(anchor)
            );
    }
}