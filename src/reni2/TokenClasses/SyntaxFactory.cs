using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;

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

                var result = factory
                    .GetSyntax(target.Left.Right)
                    .Target
                    .ToValueSyntax(target)
                    .With(factory.GetSyntax(target.Left.Right).Issues);

                NotImplementedMethod(target, factory);
                return default;
            }
        }

        class DeclarationMarkHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target, SyntaxFactory factory)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right != null);
                Tracer.Assert(target.Right.Left == null);
                Tracer.Assert(target.Right.Right == null);

                var tag = target.Right.TokenClass as DeclarationTagToken;
                Tracer.Assert(tag != null);

                return DeclarerSyntax.Tag(tag, target);
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
                => factory.ToDeclaration(target);

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

        Result<ValueSyntax> GetValueSyntax(BinaryTree target)
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

        static Result<DeclarerSyntax> ToDeclarer(Result<DeclarerSyntax> left, BinaryTree root, string name)
        {
            Tracer.Assert(!left.Issues.Any());
            return left.Target.WithName(root, name);
        }

        Result<Syntax> ToDeclaration(BinaryTree target)
        {
            var declarer = GetDeclarerSyntax(target.Left);
            var value = GetValueSyntax(target.Right);

            Tracer.Assert(!declarer.Issues.Any());
            Tracer.Assert(!value.Issues.Any());
            return new DeclarationSyntax(declarer.Target, target, value.Target, this);
        }
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