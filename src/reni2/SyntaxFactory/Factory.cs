using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

        internal static readonly IDeclarerProvider DeclarationMark = new DeclarationMarkHandler();
        internal static readonly IDeclarerProvider DefinableAsDeclarer = new DefinableHandler();
        internal static readonly IDeclarerProvider Declarer = new DeclarerHandler();
        internal static readonly IDeclarerProvider ComplexDeclarer = new ComplexDeclarerHandler();

        internal static readonly Factory Root = new Factory(false);
        static readonly IDeclarerProvider InvalidTokenDeclarer = new InvalidTokenDeclarerHandler();

        [EnableDump]
        readonly bool MeansPublic;

        Factory(bool meansPublic) => MeansPublic = meansPublic;

        internal Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target, Func<Result<DeclarerSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            return ((target.TokenClass as IDeclarerToken)?.Provider ?? InvalidTokenDeclarer)
                .Get(target, GetCurrentFactory(target));
        }

        internal Result<ValueSyntax> GetFrameSyntax(BinaryTree target)
        {
            var kernel = target.BracketKernel;
            return kernel
                .Apply(kernel => GetStatementsSyntax(null, kernel.Center, null))
                .Apply(statements=>(ValueSyntax)CompoundSyntax.Create(statements, null, kernel.Target.ToFrameItems));
        }

        internal Result<IStatementSyntax[]> GetStatementsSyntax(BinaryTree leftAnchor, BinaryTree target, FrameItemContainer frameItems)
        {
            if(target == null)
                return new IStatementSyntax[0];

            if(leftAnchor != null)
                (leftAnchor.Token.Characters < target.Token.Characters).Assert();

            return GetSyntax(
                leftAnchor
                , target
                , null
                , (node, leftAnchor) => T((IStatementSyntax)node)
                , (node, leftAnchor, rightAnchor) => T(node)
                , (node, rightAnchor) => node
                , frameItems
                );
        }

        internal Result<ValueSyntax> GetValueSyntax
        (
            BinaryTree leftAnchor
            , BinaryTree target
            , BinaryTree rightAnchor
        )
        {
            var frameItems = FrameItemContainer.Create(leftAnchor, rightAnchor);
            if(target == null)
                return new EmptyList(leftAnchor, rightAnchor, frameItems);

            return GetSyntax(
                leftAnchor
                , target
                , rightAnchor
                , (node, leftAnchor) => node
                , (node, leftAnchor, rightAnchor) => node.ToValueSyntax(leftAnchor, rightAnchor, frameItems)
                , (node, rightAnchor) => (ValueSyntax)CompoundSyntax.Create(node, null, null), frameItems);
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target)
            => target == null? null : GetValueSyntax(null, target, null);

        Result<TResult> GetSyntax<TResult>
        (
            BinaryTree leftAnchor
            , BinaryTree target
            , BinaryTree rightAnchor
            , Func<ValueSyntax, BinaryTree, Result<TResult>> fromValueSyntax
            , Func<IStatementSyntax, BinaryTree, BinaryTree, Result<TResult>> fromDeclarationSyntax
            , Func<IStatementSyntax[], BinaryTree, TResult> fromStatementsSyntax
            , FrameItemContainer frameItems
        )
            where TResult : class
        {
            if(leftAnchor != null)
                (leftAnchor.Token.Characters < target.Token.Characters).Assert();
            if(rightAnchor != null)
                (target.Token.Characters < rightAnchor.Token.Characters).Assert();

            var factory = GetCurrentFactory(target);
            var valueToken = target.TokenClass as IValueToken;
            var declarationToken = target.TokenClass as IDeclarationToken;
            var statementsToken = target.TokenClass as IStatementsToken;

            (T((object)valueToken, statementsToken, declarationToken).Count(n => n != null) <= 1).Assert();

            if(valueToken != null)
                return valueToken
                    .Provider
                    .Get(leftAnchor, target, rightAnchor, factory, frameItems)
                    .Apply(node => fromValueSyntax(node, leftAnchor));

            if(declarationToken != null)
                return declarationToken
                    .Provider
                    .Get(target, factory)
                    .Apply(node => fromDeclarationSyntax(node, leftAnchor, rightAnchor));

            if(statementsToken != null)
                return statementsToken
                    .Provider
                    .Get(leftAnchor, target, factory, frameItems)
                    .Apply(node => fromStatementsSyntax(node, rightAnchor));

            return fromValueSyntax(new EmptyList(target), leftAnchor)
                .With(IssueId.InvalidExpression.Issue(target.Token.Characters));
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

        internal Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
        {
            var namePart = DeclarerSyntax.FromName(target, name, MeansPublic);
            return left == null
                ? namePart
                : left.Apply(left => left.Combine(namePart));
        }

        internal Result<DeclarerSyntax> ToDeclarer(BinaryTree root, BinaryTree target)
        {
            if(target.TokenClass is IDeclarerToken token)
                return token.Provider.Get(target, this);

            NotImplementedMethod(root, target);
            return default;
        }

        internal Result<DeclarerSyntax> GetDeclarationTag(BinaryTree target)
        {
            (target.Right == null).Assert();

            var tag = target.TokenClass as DeclarationTagToken;
            var result = DeclarerSyntax.FromTag(tag, target, MeansPublic);

            IEnumerable<Issue> GetIssues()
            {
                if(tag == null)
                    yield return IssueId.InvalidDeclarationTag.Issue(target.Token.Characters);
            }

            return new Result<DeclarerSyntax>(result, GetIssues().ToArray());
        }

        internal Result<ExpressionSyntax> GetExpressionSyntax(BinaryTree target, FrameItemContainer frameItems)
        {
            var definable = target.TokenClass as Definable;
            (definable != null).Assert();
            return
                (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => ExpressionSyntax.Create(target, left, definable, right, frameItems));
        }

        internal Result<ValueSyntax> GetInfixSyntax
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, FrameItemContainer brackets)
            => (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => left.GetInfixSyntax(target, right, brackets));

        internal Result<DeclarerSyntax> GetDeclarationTags(BinaryTree.BracketNodes target)
            => target
                .Center
                .GetNodesFromLeftToRight()
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => left.Combine(right));
    }
}