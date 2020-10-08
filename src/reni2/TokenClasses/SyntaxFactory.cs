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

        internal interface IDeclarerProvider
        {
            Result<DeclarerSyntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IProvider
        {
            Result<Syntax> Get(BinaryTree target, SyntaxFactory factory);
        }

        internal interface IToken
        {
            IProvider Provider { get; }
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
                    .Apply(tree => factory.GetSyntax(tree, () => EmptyListIfNull(target)))
                    .Apply(syntax => syntax.ToValueSyntax(target));
        }

        class ListHandler : DumpableObject, IProvider
        {
            Result<Syntax> IProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                var result =
                    (
                        factory.GetCompoundSyntax(target.Left),
                        factory.GetCompoundSyntax(target.Right)
                    )
                    .Apply((left, right) => CompoundSyntax.Combine(left, right, target));
                return Result<Syntax>.From(result);
            }
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

        class ColonHandler : DumpableObject, IProvider
        {
            Result<Syntax> IProvider.Get(BinaryTree target, SyntaxFactory factory)
                => Result<Syntax>.From(factory.ToDeclaration(target));
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

        class TerminalHandler : DumpableObject, IValueProvider
        {
            Result<ValueSyntax> IValueProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right == null);
                return new TerminalSyntax((ITerminal)target.TokenClass, target);
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
        internal static readonly IValueProvider Terminal = new TerminalHandler();

        internal static readonly IProvider List = new ListHandler();
        internal static readonly IProvider Colon = new ColonHandler();

        internal static readonly IDeclarerProvider DeclarationMark = new DeclarationMarkHandler();
        internal static readonly IDeclarerProvider DefinableAsDeclarer = new DefinableHandler();
        internal static readonly IDeclarerProvider Declarer = new DeclarerHandler();
        internal static readonly IDeclarerProvider ComplexDeclarer = new ComplexDeclarerHandler();

        internal static readonly SyntaxFactory Root = new SyntaxFactory(false);

        [EnableDump]
        readonly bool MeansPublic;

        SyntaxFactory(bool meansPublic) => MeansPublic = meansPublic;

        bool IDefaultScopeProvider.MeansPublic => MeansPublic;

        static Result<Syntax> EmptyListIfNull(BinaryTree target) => new EmptyList(target);

        Result<CompoundSyntax> GetCompoundSyntax(BinaryTree target)
            => GetSyntax(target)
                .Apply(syntax => syntax.ToCompoundSyntax());

        Result<Syntax> GetSyntax(BinaryTree target, Func<Result<Syntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            if(target.TokenClass is IToken token)
                return token.Provider.Get(target, GetCurrentFactory(target.TokenClass));

            if(target.TokenClass is IValueToken valueToken)
                return Result<Syntax>.From(valueToken.Provider.Get(target, GetCurrentFactory(target.TokenClass)));

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

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target, Func<Result<ValueSyntax>> onNull = null)
        {
            if(target == null)
                return onNull?.Invoke();

            if(target.TokenClass is IValueToken token)
                return token.Provider.Get(target, GetCurrentFactory(target.TokenClass));


            NotImplementedMethod(target, onNull);
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

        Result<DeclarationSyntax> ToDeclaration(BinaryTree target)
            =>
                (
                    GetDeclarerSyntax(target.Left),
                    target.Right == null? null : GetValueSyntax(target.Right)
                )
                .Apply(
                    (declarerSyntax, valueSyntax) => new DeclarationSyntax(declarerSyntax, target, valueSyntax));

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
                    target.Left == null? null : GetValueSyntax(target.Left),
                    target.Right == null? null : GetValueSyntax(target.Right)
                )
                .Apply((left, right) => ExpressionSyntax.Create(target, left, definable, right));
        }

        Result<DeclarerSyntax> GetDeclarationTags(BinaryTree target)
            => target
                .Items
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => Combine(left, right, target));
    }
}