using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class SyntaxFactory : DumpableObject, IDefaultScopeProvider
    {
        class BracketHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target, SyntaxFactory factory)
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

        class DeclarationMarkHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Left == null);
                var right = target.Right;
                Tracer.Assert(right != null);
                Tracer.Assert(right.Left == null);
                Tracer.Assert(right.Right == null);

                var tag = right.TokenClass as DeclarationTagToken;
                var result = DeclarerSyntax.Tag(tag, target);

                IEnumerable<Issue> GetIssues()
                {
                    if(tag == null)
                        yield return IssueId.InvalidDeclarationTag.Issue(right.Token.Characters);
                }

                return new Result<DeclarerSyntax>(result, GetIssues().ToArray());
            }

            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }


            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class ColonHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target, SyntaxFactory factory)
                => Result<Syntax>.From(factory.ToDeclaration(target));

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class DefinableHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Right == null);

                var left = target.Left == null? null : factory.GetDeclarerSyntax(target.Left);
                return ToDeclarer(left, target, target.Token.Characters.Id);
            }

            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class TerminalHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target, SyntaxFactory factory)
            {
                NotImplementedMethod(target, factory);
                return default;
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right == null);
                return new TerminalSyntax((ITerminal)target.TokenClass, target);
            }
        }

        internal static readonly ISyntaxFactory Bracket = new BracketHandler();
        internal static readonly ISyntaxFactory DeclarationMark = new DeclarationMarkHandler();
        internal static readonly ISyntaxFactory Colon = new ColonHandler();
        internal static readonly ISyntaxFactory Definable = new DefinableHandler();
        internal static readonly ISyntaxFactory Terminal = new TerminalHandler();
        internal static readonly SyntaxFactory Root = new SyntaxFactory();

        bool IDefaultScopeProvider.MeansPublic => true;

        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetDeclarerSyntax(target, this);

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        internal Result<ValueSyntax> GetValueSyntax(BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetValueSyntax(target, this);

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        internal Result<Syntax> GetSyntax(BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetSyntax(target, this);

            NotImplementedMethod(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        static Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree target, string name)
            => left.Apply(x => x.WithName(target, name));

        Result<DeclarationSyntax> ToDeclaration(BinaryTree target)
            => (GetDeclarerSyntax(target.Left), GetValueSyntax(target.Right))
                .Apply(
                    (declarerSyntax, valueSyntax) => new DeclarationSyntax(declarerSyntax, target, valueSyntax, this));
    }

    interface ISyntaxFactory
    {
        Result<ValueSyntax> GetValueSyntax(BinaryTree target, SyntaxFactory factory);
        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory);
        Result<Syntax> GetSyntax(BinaryTree target, SyntaxFactory factory);
    }

    interface ISyntaxFactoryToken
    {
        ISyntaxFactory Provider { get; }
    }
}