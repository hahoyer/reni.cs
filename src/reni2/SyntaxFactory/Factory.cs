using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Helper;
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

        [EnableDump]
        internal readonly bool MeansPublic;

        Factory(bool meansPublic) => MeansPublic = meansPublic;

        internal ValueSyntax GetFrameSyntax(BinaryTree target)
        {
            var kernel = target.BracketKernel;
            var statements = GetStatementsSyntax(kernel.Center);
            return CompoundSyntax.Create(statements, null, Anchor.Create(kernel.Left));
        }

        internal IStatementSyntax[] GetStatementsSyntax(BinaryTree target, Anchor frameItems = null)
        {
            if(target == null)
                return new IStatementSyntax[0];

            return GetSyntax(target, node => T((IStatementSyntax)node)
                , node => T(node)
                , node => node
                , frameItems
            );
        }

        internal ValueSyntax GetValueSyntax(BinaryTree target, Anchor frameItems)
        {
            if(target == null)
                return new EmptyList(frameItems);

            var n = target
                .GetNodesFromLeftToRight()
                .FirstOrDefault(node => node.ObjectId == 32);
            var path = target.GetPath(node => node == n);
            //Tracer.ConditionalBreak(n!= null && path.Length ==0);

            return GetSyntax(target, node => node
                , node => node.ToValueSyntax()
                , node => (ValueSyntax)CompoundSyntax.Create(node)
                , frameItems);
        }

        internal ValueSyntax GetValueSyntax(BinaryTree target)
            => target == null? null : GetValueSyntax(target, null);

        TResult GetSyntax<TResult>
        (
            BinaryTree target
            , Func<ValueSyntax, TResult> fromValueSyntax
            , Func<IStatementSyntax, TResult> fromDeclarationSyntax
            , Func<IStatementSyntax[], TResult> fromStatementsSyntax
            , Anchor frameItems
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
                return fromDeclarationSyntax(declarationToken.Provider.Get(target, factory, frameItems));

            if(statementsToken != null)
                return fromStatementsSyntax(statementsToken
                    .Provider
                    .Get(target, factory, frameItems));

            return fromValueSyntax(new EmptyList(frameItems
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

        internal ExpressionSyntax GetExpressionSyntax(BinaryTree target, Anchor frameItems)
        {
            var definable = target.TokenClass as Definable;
            (definable != null).Assert();
            return ExpressionSyntax
                .Create
                (
                    GetValueSyntax(target.Left),
                    definable,
                    GetValueSyntax(target.Right),
                    Anchor.Create(target).Combine(frameItems)
                );
        }

        internal ValueSyntax GetInfixSyntax(BinaryTree target, Anchor anchor) 
            => GetValueSyntax(target.Left)
            .GetInfixSyntax(target, GetValueSyntax(target.Right), Anchor.Create(target).Combine(anchor));

        internal DeclarerSyntax CombineWithSuffix(IEnumerable<BinaryTree> nodes)
        {
            var head = nodes.First();
            var tag = head.TokenClass is IDeclarationTagToken;
            tag.Assert();
            var frameItems = Anchor.Create(nodes.Skip(1));
            return DeclarerSyntax.GetDeclarationTag(head, MeansPublic, frameItems);
        }
    }
}