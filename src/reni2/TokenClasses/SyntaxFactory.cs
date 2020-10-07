using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class SyntaxFactory : DumpableObject, IDefaultScopeProvider
    {
        class BracketHandler : DumpableObject, IValueSyntaxFactory
        {
            Result<ValueSyntax> IValueSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
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

        class ListHandler : DumpableObject, ISyntaxFactory
        {
            Result<Syntax> ISyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class DeclarationMarkHandler : DumpableObject, IDeclarerSyntaxFactory
        {
            Result<DeclarerSyntax> IDeclarerSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
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

        class ColonHandler : DumpableObject, ISyntaxFactory
        {
            Result<Syntax> ISyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
                => Result<Syntax>.From(factory.ToDeclaration(target));

        }

        class DefinableHandler : DumpableObject, IDeclarerSyntaxFactory, IValueSyntaxFactory
        {
            Result<DeclarerSyntax> IDeclarerSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Right == null);

                var left = target.Left == null? null : factory.GetDeclarerSyntax(target.Left);
                return ToDeclarer(left, target, target.Token.Characters.Id);
            }

            Result<ValueSyntax> IValueSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class TerminalHandler : DumpableObject, IValueSyntaxFactory
        {
            Result<ValueSyntax> IValueSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right == null);
                return new TerminalSyntax((ITerminal)target.TokenClass, target);
            }
        }

        class DeclarerHandler : DumpableObject, IDeclarerSyntaxFactory
        {
            Result<DeclarerSyntax> IDeclarerSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory) 
                => GetDeclarationTag(target);
        }

        class ComplexDeclarerHandler : DumpableObject, IDeclarerSyntaxFactory
        {
            Result<DeclarerSyntax> IDeclarerSyntaxFactory.Get(BinaryTree target, SyntaxFactory factory)
                => target
                    .GetBracketKernel()
                    .Apply(GetDeclarationTags);

            static Result<DeclarerSyntax> GetDeclarationTags(BinaryTree target)
                => target
                    .Items
                    .Select(GetDeclarationTag)
                    .Aggregate(DeclarerSyntax.Empty, (result1, result2) => Combine(result1, result2, target));
        }

        internal static readonly IValueSyntaxFactory Bracket = new BracketHandler();
        internal static readonly IValueSyntaxFactory Definable = new DefinableHandler();
        internal static readonly IValueSyntaxFactory Terminal = new TerminalHandler();

        internal static readonly ISyntaxFactory List = new ListHandler();
        internal static readonly ISyntaxFactory Colon = new ColonHandler();
        
        internal static readonly IDeclarerSyntaxFactory DeclarationMark = new DeclarationMarkHandler();
        internal static readonly IDeclarerSyntaxFactory DefinableAsDeclarer = new DefinableHandler();
        internal static readonly IDeclarerSyntaxFactory Declarer = new DeclarerHandler();
        internal static readonly IDeclarerSyntaxFactory ComplexDeclarer = new ComplexDeclarerHandler();

        internal static readonly SyntaxFactory Root = new SyntaxFactory(false);

        readonly bool MeansPublic;
        SyntaxFactory(bool meansPublic) => MeansPublic = meansPublic;

        bool IDefaultScopeProvider.MeansPublic => MeansPublic;

        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target)
        {
            if(target.TokenClass is IDeclarerSyntaxFactoryToken token)
                return token.Provider.Get(target, GetActualFactory(target.TokenClass));

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target)
        {
            if(target.TokenClass is IValueSyntaxFactoryToken token)
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
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.Get(target, GetActualFactory(target.TokenClass));

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        static Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
            => left.Apply(x => x.WithName(target, name));

        Result<DeclarationSyntax> ToDeclaration(BinaryTree target)
            => (GetDeclarerSyntax(target.Left), GetValueSyntax(target.Right))
                .Apply(
                    (declarerSyntax, valueSyntax) => new DeclarationSyntax(declarerSyntax, target, valueSyntax, this));

        Result<DeclarerSyntax> ToDeclarer(BinaryTree root, BinaryTree target)
        {
            if(target.TokenClass is IDeclarerSyntaxFactoryToken token)
                return token.Provider.Get(target, this);

            NotImplementedMethod(root, target);
            return default;
        }

        static Result<DeclarerSyntax> GetDeclarationTag(BinaryTree target)
        {
            Tracer.Assert(target.Right == null);

            var tag = target.TokenClass as DeclarationTagToken;
            var result = DeclarerSyntax.Tag(tag, target);

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
    }

    interface IDeclarerSyntaxFactoryToken
    {
        IDeclarerSyntaxFactory Provider { get; }
    }

    interface IDeclarerSyntaxFactory
    {
        Result<DeclarerSyntax> Get(BinaryTree target, SyntaxFactory factory);
    }

    interface ISyntaxFactory
    {
        Result<Syntax> Get(BinaryTree target, SyntaxFactory factory);
    }

    interface ISyntaxFactoryToken
    {
        ISyntaxFactory Provider { get; }
    }

    interface IValueSyntaxFactory
    {
        Result<ValueSyntax> Get(BinaryTree target, SyntaxFactory factory);
    }

    interface IValueSyntaxFactoryToken
    {
        IValueSyntaxFactory Provider { get; }
    }
}