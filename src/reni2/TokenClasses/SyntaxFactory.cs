using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class SyntaxFactory : DumpableObject, IDefaultScopeProvider
    {
        internal interface IDeclarerToken
        {
            IDeclarerProvider Provider { get; }
        }

        internal interface IDeclarationToken
        {
            IStatementProvider Provider { get; }
        }

        internal interface IStatementsToken
        {
            IStatementsProvider Provider { get; }
        }

        internal interface IDeclarerProvider
        {
            Result<DeclarerSyntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IStatementsProvider
        {
            Result<StatementSyntax[]> Get(BinaryTree anchor, BinaryTree target, SyntaxFactory factory);
        }

        internal interface IStatementProvider
        {
            Result<IStatementSyntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IValueProvider
        {
            Result<ValueSyntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IValueToken
        {
            IValueProvider Provider { get; }
        }

        class BracketHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                => target
                    .GetBracketKernel()
                    .Apply(tree => factory.GetValueSyntax(tree, target, () => EmptyListIfNull(target)));
        }

        class FrameHandler : DumpableObject, IStatementsProvider
        {
            Result<StatementSyntax[]> IStatementsProvider.Get
                (BinaryTree anchor, BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(anchor == null);
                return target
                    .GetBracketKernel()
                    .Apply(kernel => factory.GetStatementsSyntax(target.Left, kernel));
            }
        }

        class ListHandler : DumpableObject, IStatementsProvider
        {
            Result<StatementSyntax[]> IStatementsProvider.Get
                (BinaryTree anchor, BinaryTree target, SyntaxFactory factory)
                => (
                        factory.GetStatementsSyntax(anchor, target.Left),
                        factory.GetStatementsSyntax(target, target.Right)
                    )
                    .Apply((left, right) => left.Concat(right).ToArray());
        }

        class ColonHandler : DumpableObject, IStatementProvider
        {
            Result<IStatementSyntax> IStatementProvider.Get(BinaryTree target, SyntaxFactory factory)
                =>
                    (
                        factory.GetDeclarerSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply
                    ((declarer, value) => DeclarationSyntax.Create(declarer, target, value)
                    );
        }

        class CleanupHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                =>
                    (
                        factory.GetStatementsSyntax(target, target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply
                    ((statements, cleanup)
                        => (ValueSyntax)new CompoundSyntax(statements, target, cleanup)
                    );
        }

        class DeclarationMarkHandler : DumpableObject, IDeclarerProvider
        {
            Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Right != null);

                return
                    (
                        factory.GetDeclarerSyntax(target.Left),
                        factory.ToDeclarer(target, target.Right)
                    )
                    .Apply((left, other) => Combine(left, other));
            }
        }

        class DefinableHandler : DumpableObject, IDeclarerProvider, IValueProvider
        {
            Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Right == null);

                return factory.ToDeclarer(factory.GetDeclarerSyntax(target.Left), target, target.Token.Characters.Id);
            }

            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                => Result<ValueSyntax>.From(factory.GetExpressionSyntax(target));
        }

        class InfixHandler : DumpableObject, IValueProvider
        {
            readonly bool? HasLeft;
            readonly bool? HasRight;

            public InfixHandler(bool? hasLeft = null, bool? hasRight = null)
            {
                HasLeft = hasLeft;
                HasRight = hasRight;
            }

            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                if(HasLeft != null)
                    Tracer.Assert(target.Left == null != HasLeft);
                if(HasRight != null)
                    Tracer.Assert(target.Right == null != HasRight);

                return factory.GetInfixSyntax(target);
            }
        }

        class FunctionHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                var token = (Function)target.TokenClass;
                return (
                        factory.GetValueSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply((setter, getter)
                        => (ValueSyntax)new FunctionSyntax
                        (
                            setter
                            , token.IsImplicit
                            , token.IsMetaFunction
                            , getter
                            , target
                        )
                    );
            }
        }

        class MatchedBracketHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                => (
                        factory.GetValueSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply((left, right)
                        => (ValueSyntax)new ExpressionSyntax(target, left, null, right));
        }

        class ThenHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                => (
                        factory.GetValueSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply((condition, thenClause)
                        => (ValueSyntax)new CondSyntax(condition, thenClause, null, target));
        }

        class ElseHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                => (
                        factory.GetValueSyntax(target.Left?.Left),
                        factory.GetValueSyntax(target.Left?.Right),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply((condition, thenClause, elseClause)
                        => (ValueSyntax)new CondSyntax(condition, thenClause, elseClause, target));
        }

        class DeclarerHandler : DumpableObject, IDeclarerProvider
        {
            Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, SyntaxFactory factory)
                => factory.GetDeclarationTag(target);
        }

        class ComplexDeclarerHandler : DumpableObject, IDeclarerProvider
        {
            Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, SyntaxFactory factory)
                => target
                    .GetBracketKernel()
                    .Apply(factory.GetDeclarationTags);
        }

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

        internal static readonly IStatementsProvider Frame = new FrameHandler();
        internal static readonly IStatementsProvider List = new ListHandler();
        internal static readonly IStatementProvider Colon = new ColonHandler();

        internal static readonly IDeclarerProvider DeclarationMark = new DeclarationMarkHandler();
        internal static readonly IDeclarerProvider DefinableAsDeclarer = new DefinableHandler();
        internal static readonly IDeclarerProvider Declarer = new DeclarerHandler();
        internal static readonly IDeclarerProvider ComplexDeclarer = new ComplexDeclarerHandler();

        internal static readonly SyntaxFactory Root = new SyntaxFactory(false);

        [EnableDump]
        readonly bool MeansPublic;


        SyntaxFactory(bool meansPublic) => MeansPublic = meansPublic;

        bool IDefaultScopeProvider.MeansPublic => MeansPublic;

        static Result<ValueSyntax> EmptyListIfNull(BinaryTree target) => new EmptyList(target);

        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target, Func<Result<DeclarerSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            if(target.TokenClass is IDeclarerToken token)
                return token.Provider.Get(target, GetCurrentFactory(target.TokenClass));

            NotImplementedMethod(target, onNull);
            return default;
        }

        Result<StatementSyntax[]> GetStatementsSyntax(BinaryTree anchor, BinaryTree target)
        {
            if(target == null)
                return new StatementSyntax[0];

            return GetSyntax(
                target, anchor,
                node => node.ToStatementsSyntax(),
                node => StatementSyntax.Create(anchor, node),
                node => node);
        }

        internal Result<ValueSyntax> GetValueSyntax
            (BinaryTree target, BinaryTree parent = null, Func<Result<ValueSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            return GetSyntax(
                target,
                parent,
                value => new Result<ValueSyntax>(value), ToValue,
                list => (ValueSyntax)new CompoundSyntax(list, parent));
        }

        Result<ValueSyntax> ToValue(IStatementSyntax target)
        {
            NotImplementedMethod(target);
            return default;
        }


        Result<TResult> GetSyntax<TResult>
        (
            BinaryTree target
            , BinaryTree anchor
            , Func<ValueSyntax, Result<TResult>> fromValueSyntax
            , Func<IStatementSyntax, Result<TResult>> fromDeclarationSyntax
            , Func<StatementSyntax[], TResult> fromStatementsSyntax
        )
            where TResult : class
        {
            var factory = GetCurrentFactory(target.TokenClass);
            var valueToken = target.TokenClass as IValueToken;
            var declarationToken = target.TokenClass as IDeclarationToken;
            var statementsToken = target.TokenClass as IStatementsToken;

            Tracer.Assert(T((object)valueToken, statementsToken, declarationToken).Count(n => n != null) <= 1);

            if(valueToken != null)
                return valueToken.Provider.Get(target, factory)
                    .Apply(fromValueSyntax);

            if(declarationToken != null)
                return declarationToken.Provider.Get(target, factory)
                    .Apply(fromDeclarationSyntax);

            if(statementsToken != null)
                return statementsToken.Provider.Get(anchor, target, factory)
                    .Apply(fromStatementsSyntax);

            return fromValueSyntax(new EmptyList(target))
                .With(IssueId.InvalidExpression.Issue(target.Token.Characters));
        }

        SyntaxFactory GetCurrentFactory(ITokenClass tokenClass)
        {
            var meansPublic = (tokenClass as IDefaultScopeProvider)?.MeansPublic ?? MeansPublic;
            return meansPublic == MeansPublic? this : new SyntaxFactory(meansPublic);
        }

        Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
        {
            var namePart = DeclarerSyntax.FromName(target, name, MeansPublic);
            return left == null
                ? namePart
                : left.Apply(left => left.Combine(namePart));
        }

        Result<DeclarerSyntax> ToDeclarer(BinaryTree root, BinaryTree target)
        {
            if(target.TokenClass is IDeclarerToken token)
                return token.Provider.Get(target, this);

            NotImplementedMethod(root, target);
            return default;
        }

        Result<DeclarerSyntax> GetDeclarationTag(BinaryTree target)
        {
            Tracer.Assert(target.Right == null);

            var tag = target.TokenClass as DeclarationTagToken;
            var result = DeclarerSyntax.FromTag(tag, target, MeansPublic);

            IEnumerable<Issue> GetIssues()
            {
                if(tag == null)
                    yield return IssueId.InvalidDeclarationTag.Issue(target.Token.Characters);
            }

            return new Result<DeclarerSyntax>(result, GetIssues().ToArray());
        }

        static Result<DeclarerSyntax> Combine
            (Result<DeclarerSyntax> target, Result<DeclarerSyntax> other, BinaryTree root = null)
            => (target, other)
                .Apply((target, other) => target?.Combine(other, root) ?? other);

        Result<ExpressionSyntax> GetExpressionSyntax(BinaryTree target)
        {
            var definable = target.TokenClass as Definable;
            Tracer.Assert(definable != null);
            return
                (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => ExpressionSyntax.Create(target, left, definable, right));
        }

        Result<ValueSyntax> GetInfixSyntax(BinaryTree target)
            => (
                    GetValueSyntax(target.Left),
                    GetValueSyntax(target.Right)
                )
                .Apply((left, right) => GetInfixSyntax(left, target, right));

        static Result<ValueSyntax> GetInfixSyntax(ValueSyntax left, BinaryTree target, ValueSyntax right)
            => left == null? right == null?
                    (Result<ValueSyntax>)new TerminalSyntax((ITerminal)target.TokenClass, target) :
                    new PrefixSyntax((IPrefix)target.TokenClass, right, target) :
                right == null? (Result<ValueSyntax>)new SuffixSyntax(left, (ISuffix)target.TokenClass, target) :
                new InfixSyntax(left, (IInfix)target.TokenClass, right, target);

        Result<DeclarerSyntax> GetDeclarationTags(BinaryTree target)
            => target
                .GetNodesFromLeftToRight()
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => Combine(left, right));
    }
}