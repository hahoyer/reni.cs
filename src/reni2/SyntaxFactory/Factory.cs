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

        internal Result<StatementSyntax[]> GetStatementsSyntax(BinaryTree leftAnchor, BinaryTree target)
        {
            if(target == null)
                return new StatementSyntax[0];

            if(leftAnchor != null)
                (leftAnchor.Token.Characters < target.Token.Characters).Assert();

            return GetSyntax(
                leftAnchor
                , target
                , null
                , node => StatementSyntax.Create(leftAnchor, node)
                , node => StatementSyntax.Create(leftAnchor, node)
                , node => node
            );
        }


        internal Result<ValueSyntax> GetFrameSyntax(BinaryTree target)
            => target.BracketKernel
                .Apply(kernel => GetStatementsSyntax(kernel.Left, kernel.Center))
                .Apply(statements => (ValueSyntax)new CompoundSyntax(statements, null, target));

        internal Result<ValueSyntax> GetValueSyntax
        (
            BinaryTree leftAnchor
            , BinaryTree target
            , BinaryTree rightAnchor
        )
        {
            if(target == null)
                return new BracketSyntax(leftAnchor, null, rightAnchor);

            leftAnchor.AssertIsNotNull();
            rightAnchor.AssertIsNotNull();
            (leftAnchor.Token.Characters < target.Token.Characters).Assert();
            (target.Token.Characters < rightAnchor.Token.Characters).Assert();

            return GetSyntax(
                leftAnchor
                , target
                , rightAnchor
                , node => new BracketSyntax(leftAnchor, node, rightAnchor)
                , node => node.ToValueSyntax(leftAnchor, rightAnchor)
                , node => (ValueSyntax)new CompoundSyntax(node, null, rightAnchor)
            );
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target)
        {
            if(target == null)
                return null;

            return GetSyntax(
                null
                , target
                , null
                , node => new Result<ValueSyntax>(node)
                , node => node.ToValueSyntax(null, null)
                , node => (ValueSyntax)new CompoundSyntax(node, null, null)
            );
        }

        Result<TResult> GetSyntax<TResult>
        (
            BinaryTree leftAnchor
            , BinaryTree target
            , BinaryTree rightAnchor
            , Func<ValueSyntax, Result<TResult>> fromValueSyntax
            , Func<IStatementSyntax, Result<TResult>> fromDeclarationSyntax
            , Func<StatementSyntax[], TResult> fromStatementsSyntax
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
                    .Get(leftAnchor, target, rightAnchor, factory)
                    .Apply(fromValueSyntax);

            if(declarationToken != null)
                return declarationToken
                    .Provider
                    .Get(target, factory)
                    .Apply(fromDeclarationSyntax);

            if(statementsToken != null)
                return statementsToken
                    .Provider
                    .Get(leftAnchor, target, factory)
                    .Apply(fromStatementsSyntax);

            return fromValueSyntax(new EmptyList(target))
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

        internal Result<ExpressionSyntax> GetExpressionSyntax(BinaryTree target)
        {
            var definable = target.TokenClass as Definable;
            (definable != null).Assert();
            return
                (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => ExpressionSyntax.Create(target, left, definable, right));
        }

        internal Result<ValueSyntax> GetInfixSyntax(BinaryTree target)
            => (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => left.GetInfixSyntax(target, right));

        internal Result<DeclarerSyntax> GetDeclarationTags(BinaryTree.BracketNodes target)
            => target
                .Center
                .GetNodesFromLeftToRight()
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => left.Combine(right));

    }
}