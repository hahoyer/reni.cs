using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    class Factory : DumpableObject
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
        static readonly IDeclarerProvider InvalidTokenDeclarer = new InvalidTokenDeclarerHandler();

        [EnableDump]
        internal readonly bool MeansPublic;

        Factory(bool meansPublic) => MeansPublic = meansPublic;

        internal ValueSyntax GetFrameSyntax(BinaryTree target)
        {
            var kernel = target.BracketKernel;
            var statements = GetStatementsSyntax(kernel.Center, FrameItemContainer.Create());
            return CompoundSyntax.Create(statements, null, kernel.ToFrameItems);
        }

        internal IStatementSyntax[] GetStatementsSyntax(BinaryTree target, FrameItemContainer frameItems)
        {
            if(target == null)
                return new IStatementSyntax[0];

            return GetSyntax(target, node => T((IStatementSyntax)node)
                , (node, _) => T(node)
                , (node, _) => node
                , frameItems
            );
        }

        internal ValueSyntax GetValueSyntax(BinaryTree target, FrameItemContainer frameItems)
        {
            if(target == null)
                return new EmptyList(frameItems);

            return GetSyntax(target, node => node
                , (node, frameItems) => node.ToValueSyntax(frameItems)
                , (node, frameItems) => (ValueSyntax)CompoundSyntax.Create(node, null, frameItems)
                , frameItems);
        }

        internal ValueSyntax GetValueSyntax(BinaryTree target)
            => target == null? null : GetValueSyntax(target, FrameItemContainer.Create(null, null));

        TResult GetSyntax<TResult>
        (
            BinaryTree target
            , Func<ValueSyntax, TResult> fromValueSyntax
            , Func<IStatementSyntax, FrameItemContainer, TResult> fromDeclarationSyntax
            , Func<IStatementSyntax[], FrameItemContainer, TResult> fromStatementsSyntax
            , FrameItemContainer frameItems
        )
            where TResult : class
        {
            var factory = GetCurrentFactory(target);
            var valueToken = target.TokenClass as IValueToken;
            var declarationToken = target.TokenClass as IDeclarationToken;
            var statementsToken = target.TokenClass as IStatementsToken;

            (T((object)valueToken, statementsToken, declarationToken).Count(n => n != null) <= 1).Assert();

            if(valueToken != null)
                return fromValueSyntax(valueToken.Provider.Get(target, factory, frameItems));

            if(declarationToken != null)
                return fromDeclarationSyntax(declarationToken.Provider.Get(target, factory), frameItems);

            if(statementsToken != null)
                return statementsToken
                    .Provider
                    .Get(target, factory, FrameItemContainer.Create())
                    .StripIssues(node => fromStatementsSyntax(node, frameItems));

            return fromValueSyntax(new EmptyList(target, frameItems
                , IssueId.InvalidExpression.Issue(target.Token.Characters)));
        }

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

        internal DeclarerSyntax ToDeclarer(DeclarerSyntax left, BinaryTree target, string name, Issue issue = null)
        {
            var namePart = DeclarerSyntax.FromName(target, name, MeansPublic);
            return left == null
                ? namePart
                : left.Combine(namePart);
        }

        internal DeclarerSyntax GetDeclarationTag(BinaryTree target, FrameItemContainer frameItems)
        {
            (target.Right == null).Assert();
            switch(target.TokenClass)
            {
                case DeclarationTagToken tag:
                    return DeclarerSyntax.FromTag(tag, target, MeansPublic, frameItems);
                case TokenClasses.Definable:
                    return DeclarerSyntax.FromName(target, target.Token.Characters.Id, MeansPublic, frameItems);
                default:
                    NotImplementedMethod(target, frameItems);
                    return default;



                    //var issue = tag == null? null : IssueId.InvalidDeclarationTag.Issue(target.Token.Characters);
            }
        }

        internal ExpressionSyntax GetExpressionSyntax(BinaryTree target, FrameItemContainer frameItems)
        {
            var definable = target.TokenClass as Definable;
            (definable != null).Assert();
            return ExpressionSyntax
                .Create(target, GetValueSyntax(target.Left), definable, GetValueSyntax(target.Right), frameItems);
        }

        internal ValueSyntax GetInfixSyntax(BinaryTree target, FrameItemContainer frameItems)
            => GetValueSyntax(target.Left).GetInfixSyntax(target, GetValueSyntax(target.Right), frameItems);

        internal DeclarerSyntax GetDeclarationTags(BinaryTree.BracketNodes target)
            => target
                .Center
                .GetNodesFromLeftToRight()
                .Select(target1 => GetDeclarationTag(target1, null))
                .Aggregate(DeclarerSyntax.Empty, (left, right) => left.Combine(right));

        internal DeclarerSyntax GetDeclarationName(BinaryTree target)
            => DeclarerSyntax.FromName(target, target.Token.Characters.Id, MeansPublic);

        public DeclarerSyntax CombineWithSuffix(IEnumerable<BinaryTree> nodes)
        {
            var head = nodes.First();
            var tag = head.TokenClass is IDeclarationTagToken;
            tag.Assert();
            var frameItems = FrameItemContainer.Create(nodes.Skip(1));
            return GetDeclarationTag(head, frameItems);
        }
    }
}