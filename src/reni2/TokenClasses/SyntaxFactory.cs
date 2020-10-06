using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    static class SyntaxFactory
    {
        class BracketHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target)
            {
                NotImplementedFunction(target);
                return default;
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target)
            {
                Tracer.Assert(target.Left != null);
                Tracer.Assert(target.Right == null);
                Tracer.Assert(target.Left.Left == null);
                Tracer.Assert(target.Left.TokenClass.IsBelongingTo(target.TokenClass),
                    $"target.Left.TokenClass: {target.Left.TokenClass}  <===> target.TokenClass: {target.TokenClass}"
                );
                Tracer.Assert(target.Left.Right != null);

                var result = GetValueSyntax(target.Left.Right).ToBracketFrame(target);

                NotImplementedFunction(target);
                return default;
            }
        }

        class DeclarationMarkHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right != null);
                Tracer.Assert(target.Right.Left == null);
                Tracer.Assert(target.Right.Right == null);

                var tag = target.Right.TokenClass as DeclarationTagToken;
                Tracer.Assert(tag != null);

                return new DeclarerSyntax.Tag(tag, target);

                NotImplementedFunction(target);
                return default;
            }


            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target)
            {
                NotImplementedFunction(target);
                return default;
            }
        }

        class ColonHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target)
            {
                NotImplementedFunction(target);
                return default;
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target)
            {
                var declarer = GetDeclarerSyntax(target.Left);
                var value = GetValueSyntax(target.Right);

                return ToDeclaration(declarer, target, value);
            }
        }

        class DefinableHandler : DumpableObject, ISyntaxFactory
        {
            Result<DeclarerSyntax> ISyntaxFactory.GetDeclarerSyntax(BinaryTree target)
            {
                Tracer.Assert(target.Right == null);

                var left = target.Left == null? null : GetDeclarerSyntax(target.Left);
                return left.ToDeclarer(target, target.Token.Characters.Id);
            }

            Result<ValueSyntax> ISyntaxFactory.GetValueSyntax(BinaryTree target)
            {
                NotImplementedFunction(target);
                return default;
            }
        }

        internal static readonly ISyntaxFactory Bracket = new BracketHandler();
        internal static readonly ISyntaxFactory DeclarationMark = new DeclarationMarkHandler();
        internal static readonly ISyntaxFactory Colon = new ColonHandler();
        internal static readonly ISyntaxFactory Definable = new DefinableHandler();


        internal static Result<ValueSyntax> GetFrameSyntax(this BinaryTree target)
            => GetValueSyntax(target).ToFrame();

        static Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetDeclarerSyntax(target);

            Dumpable.NotImplementedFunction(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        static Result<ValueSyntax> GetValueSyntax(this BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetValueSyntax(target);

            Dumpable.NotImplementedFunction(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        static Result<ValueSyntax> ToFrame(this Result<ValueSyntax> target)
        {
            Dumpable.NotImplementedFunction(target);
            return default;
        }

        static Result<ValueSyntax> ToBracketFrame(this Result<ValueSyntax> target, BinaryTree root)
        {
            Dumpable.NotImplementedFunction(target, root.TokenClass);
            return default;
        }

        static Result<ValueSyntax> ToDeclaration
            (Result<DeclarerSyntax> declarer, BinaryTree root, Result<ValueSyntax> value)
        {
            Dumpable.NotImplementedFunction(declarer, root.TokenClass, value);
            return default;
        }

        static Result<DeclarerSyntax> ToDeclarer(this Result<DeclarerSyntax> left, BinaryTree root, string name)
        {

            Dumpable.NotImplementedFunction(left, root.TokenClass, name);
            return default;
        }
    }

    interface ISyntaxFactory
    {
        Result<ValueSyntax> GetValueSyntax(BinaryTree target);
        Result<DeclarerSyntax> GetDeclarerSyntax(BinaryTree target);
    }

    interface ISyntaxFactoryToken
    {
        ISyntaxFactory Provider { get; }
    }
}