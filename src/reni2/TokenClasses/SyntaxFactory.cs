using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

        internal interface IDeclarationsToken
        {
            IDeclarationsProvider Provider { get; }
        }

        internal interface IDeclarerProvider
        {
            Result<DeclarerSyntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IDeclarationsProvider
        {
            Result<DeclarationSyntax[]> Get(BinaryTree target, SyntaxFactory factory);
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
                    .Apply(tree => factory.GetValueSyntax(tree, () => EmptyListIfNull(target)));
        }

        class ListHandler : DumpableObject, IDeclarationsProvider
        {
            Result<DeclarationSyntax[]> IDeclarationsProvider.Get(BinaryTree target, SyntaxFactory factory)
                => (
                        factory.GetDeclarationsSyntax(target.Left),
                        factory.GetDeclarationsSyntax(target.Right)
                    )
                    .Apply((left, right) => left.Concat(right).ToArray());
        }

        class ColonHandler : DumpableObject, IDeclarationsProvider
        {
            Result<DeclarationSyntax[]> IDeclarationsProvider.Get(BinaryTree target, SyntaxFactory factory)
                =>
                    (
                        factory.GetDeclarerSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply
                    ((declarer, value)
                        => T(new DeclarationSyntax(declarer, target, value))
                    );
        }

        class CleanupHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
                =>
                    (
                        factory.GetDeclarationsSyntax(target.Left),
                        factory.GetValueSyntax(target.Right)
                    )
                    .Apply
                    ((statements, cleanup)
                        => (ValueSyntax)new CompoundSyntax(statements, cleanup, target)
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
                    .Apply((left, other) => Combine(left, other, target));
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
        internal static readonly IValueProvider Definable = new DefinableHandler();
        internal static readonly IValueProvider Terminal = new InfixHandler(false, false);
        internal static readonly IValueProvider Suffix = new InfixHandler(true, false);
        internal static readonly IValueProvider NonSuffix = new InfixHandler(false);
        internal static readonly IValueProvider InfixPrefix = new InfixHandler(hasRight: true);
        internal static readonly IValueProvider Infix = new InfixHandler(true, true);

        internal static readonly IDeclarationsProvider List = new ListHandler();
        internal static readonly IDeclarationsProvider Colon = new ColonHandler();
        internal static readonly IValueProvider Cleanup = new CleanupHandler();

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

        Result<Syntax> GetSyntax(BinaryTree target, Func<Result<Syntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            var valueToken = target.TokenClass as IValueToken;
            var declarationsToken = target.TokenClass as IDeclarationsToken;

            Tracer.Assert(
                (valueToken== null? 0:1)+
                (declarationsToken== null? 0:1) 
                == 1);
            
            if(valueToken != null)
                return Result<Syntax>.From(valueToken.Provider.Get(target, GetCurrentFactory(target.TokenClass)));

            if(declarationsToken != null)
                return Result<Syntax>.From(
                    declarationsToken.Provider.Get(target, GetCurrentFactory(target.TokenClass))
                        .Apply(list=> new CompoundSyntax(list, null, target)));

            NotImplementedMethod(target, onNull);
            return default;
        }

        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target, Func<Result<DeclarerSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            if(target.TokenClass is IDeclarerToken token)
                return token.Provider.Get(target, GetCurrentFactory(target.TokenClass));

            NotImplementedMethod(target, onNull);
            return default;
        }

        Result<DeclarationSyntax[]> GetDeclarationsSyntax(BinaryTree target)
        {
            if(target == null)
                return new DeclarationSyntax[0];

            return GetValueLikeSyntax(
                target,
                value => value.ToDeclarationsSyntax(),
                i => i);
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target, Func<Result<ValueSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            return GetValueLikeSyntax(
                target,
                value => value,
                list => (ValueSyntax)new CompoundSyntax(list, null, null));
        }

        Result<TResult> GetValueLikeSyntax<TResult>
            (BinaryTree target
            , Func<ValueSyntax, TResult> fromValueSyntax
            , Func<DeclarationSyntax[], TResult> fromDeclarationsSyntax
            )
            where TResult : class
        {
            var factory = GetCurrentFactory(target.TokenClass);
            var valueToken = target.TokenClass as IValueToken;
            var declarationsToken = target.TokenClass as IDeclarationsToken;

            Tracer.Assert(!(valueToken != null && declarationsToken != null));

            if(valueToken != null)
                return valueToken.Provider.Get(target, factory)
                    .Apply(fromValueSyntax);
            
            if(declarationsToken != null)
                return declarationsToken.Provider.Get(target, factory)
                    .Apply(fromDeclarationsSyntax);
            NotImplementedMethod(target, fromValueSyntax,fromDeclarationsSyntax);
            return default;
        }

        SyntaxFactory GetCurrentFactory(ITokenClass tokenClass)
        {
            var meansPublic = (tokenClass as IDefaultScopeProvider)?.MeansPublic ?? MeansPublic;
            return meansPublic == MeansPublic? this : new SyntaxFactory(meansPublic);
        }

        Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
        {
            var namePart = DeclarerSyntax.FromName(target, name, this);
            return left == null
                ? namePart
                : left.Apply(left => left.Combine(namePart, target));
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
            var result = DeclarerSyntax.FromTag(tag, target, this);

            IEnumerable<Issue> GetIssues()
            {
                if(tag == null)
                    yield return IssueId.InvalidDeclarationTag.Issue(target.Token.Characters);
            }

            return new Result<DeclarerSyntax>(result, GetIssues().ToArray());
        }

        static Result<DeclarerSyntax> Combine
            (Result<DeclarerSyntax> target, Result<DeclarerSyntax> other, BinaryTree root)
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
                .Items
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => Combine(left, right, target));
    }
}