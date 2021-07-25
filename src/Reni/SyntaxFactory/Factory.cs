using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    sealed class Factory : DumpableObject
    {
        internal static readonly IValueProvider Bracket = new BracketHandler();
        internal static readonly IValueProvider MatchedBracket = new MatchedBracketHandler();
        internal static readonly IValueProvider Definable = new DefinableHandler();
        internal static readonly IValueProvider Terminal = new InfixHandler(false, false);
        internal static readonly IValueProvider Suffix = new InfixHandler(true, false);
        internal static readonly IValueProvider NonSuffix = new InfixHandler(false);
        internal static readonly IValueProvider InfixPrefix = new InfixHandler(hasRight: true);
        internal static readonly IValueProvider Infix = new InfixHandler(true, true);
        internal static readonly IValueProvider Function = new FunctionHandler();
        internal static readonly IValueProvider Cleanup = new CleanupHandler();
        internal static readonly IValueProvider Else = new ElseHandler();
        internal static readonly IValueProvider Then = new ThenHandler();

        internal static readonly IStatementsProvider List = new ListHandler();
        internal static readonly IStatementProvider Colon = new ColonHandler();

        internal static readonly Factory Root = new Factory(false);

        [EnableDump]
        internal readonly bool MeansPublic;

        Factory(bool meansPublic) => MeansPublic = meansPublic;

        internal ValueSyntax GetFrameSyntax(BinaryTree target)
        {
            var kernel = target.BracketKernel;
            var statements = GetStatementsSyntax(kernel.Center, null, kernel.Center?.TokenClass);
            var listAnchors = kernel.Center.AssertNotNull().ParserLevelGroup;
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
                    anchor.AssertIsNull(() => anchor.Dump());
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
                case IValueToken valueToken:
                    return valueToken.Provider.Get(target, factory, anchor);
                case IDeclarationToken declarationToken:
                    return declarationToken.Provider.Get(target, factory).ToValueSyntax(anchor);
                case IStatementsToken statementsToken:
                {
                    var node = statementsToken
                        .Provider
                        .Get(target, factory);
                    anchor = Anchor.Create(target.ParserLevelGroup).Combine(anchor);
                    return CompoundSyntax.Create(node, null, anchor);
                }
                default:
                    return new EmptyList
                    (
                        Anchor.Create(target).Combine(anchor)
                        , IssueId.InvalidExpression.Issue(target.Token.Characters)
                    );
            }
        }

        internal ValueSyntax GetValueSyntax(BinaryTree target)
            => target == null? null : GetValueSyntax(target, null);

        Factory GetCurrentFactory(BinaryTree target)
        {
            var level = target.GetBracketLevel();
            if(level == null)
                return this;

            return level switch
                   {
                       0 => true, 3 => true, _ => false
                   } ==
                   MeansPublic
                ? this
                : new Factory(!MeansPublic);
        }

        internal ExpressionSyntax GetExpressionSyntax(BinaryTree target, Anchor anchor)
        {
            var definable = target.TokenClass as Definable;
            (definable != null).Assert();
            return ExpressionSyntax
                .Create
                (
                    GetValueSyntax(target.Left),
                    definable, target.Token, GetValueSyntax(target.Right), Anchor.Create(target).Combine(anchor));
        }

        internal ValueSyntax GetInfixSyntax(BinaryTree target, Anchor anchor)
            => GetValueSyntax(target.Left)
                .GetInfixSyntax(target, GetValueSyntax(target.Right), Anchor.Create(target).Combine(anchor));

    }
}