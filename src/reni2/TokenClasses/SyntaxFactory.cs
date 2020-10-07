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
            {
                Tracer.Assert(target.Left != null);
                Tracer.Assert(target.Right == null);
                Tracer.Assert(target.Left.Left == null);
                Tracer.Assert(target.Left.TokenClass.IsBelongingTo(target.TokenClass),
                    $"target.Left.TokenClass: {target.Left.TokenClass}  <===> target.TokenClass: {target.TokenClass}"
                );
                Tracer.Assert(target.Left.Right != null);

                return factory
                    .GetSyntax(target.Left.Right)
                    .Apply(syntax => syntax.ToValueSyntax(target));
            }
        }

        class ListHandler : DumpableObject, IProvider
        {
            Result<Syntax> IProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                var result =
                    (
                        target.Left == null? null : factory.GetCompoundSyntax(target.Left),
                        target.Right == null? null : factory.GetCompoundSyntax(target.Right)
                    )
                    .Apply((left, right) => CompoundSyntax.Combine(left, right, target));
                return Result<Syntax>.From(result);
            }
        }

        class DeclarationMarkHandler : DumpableObject, IDeclarerProvider
        {
            Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, SyntaxFactory factory)
            {
                var left = target.Left == null? null : factory.GetDeclarerSyntax(target.Left);
                var right = target.Right;
                Tracer.Assert(right != null);

                var result = factory
                    .ToDeclarer(target, right);

                if(left == null)
                    return result;
                return (left, result)
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

                var left = target.Left == null? null : factory.GetDeclarerSyntax(target.Left);
                return factory.ToDeclarer(left, target, target.Token.Characters.Id);
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

        Result<CompoundSyntax> GetCompoundSyntax(BinaryTree target)
            => GetSyntax(target)
                .Apply(syntax => syntax.ToCompoundSyntax());

        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target)
        {
            if(target.TokenClass is IDeclarerToken token)
                return token.Provider.Get(target, GetActualFactory(target.TokenClass));

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target)
        {
            if(target.TokenClass is IValueToken token)
                return token.Provider.Get(target, GetActualFactory(target.TokenClass));


            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        SyntaxFactory GetActualFactory(ITokenClass tokenClass)
        {
            var meansPublic = (tokenClass as IDefaultScopeProvider)?.MeansPublic ?? MeansPublic;
            return meansPublic == MeansPublic? this : new SyntaxFactory(meansPublic);
        }

        Result<Syntax> GetSyntax(BinaryTree target)
        {
            if(target.TokenClass is IToken token)
                return token.Provider.Get(target, GetActualFactory(target.TokenClass));

            if(target.TokenClass is IValueToken valueToken)
                return Result<Syntax>.From(valueToken.Provider.Get(target, GetActualFactory(target.TokenClass)));

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
        {
            var namePart = DeclarerSyntax.FromName(target, name, this);
            return left == null
                ? namePart
                : left.Apply(left => left.Combine(namePart, target));
        }

        Result<DeclarationSyntax> ToDeclaration(BinaryTree target)
            => (GetDeclarerSyntax(target.Left), GetValueSyntax(target.Right))
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
                .Apply((target, other) => target.Combine(other, root));

        Result<ExpressionSyntax> GetExpressionSyntax(BinaryTree target)
        {
            Tracer.Assert(target.Right == null);

            var definable = target.TokenClass as Definable;
            Tracer.Assert(definable != null);

            if(target.Left == null)
                return ExpressionSyntax.Create(target, null, definable, null);

            return
                GetValueSyntax(target.Left)
                    .Apply(left => ExpressionSyntax.Create(target, left, definable, null));
        }

        Result<DeclarerSyntax> GetDeclarationTags(BinaryTree target)
            => target
                .Items
                .Select(GetDeclarationTag)
                .Aggregate(DeclarerSyntax.Empty, (left, right) => Combine(left, right, target));
    }
}