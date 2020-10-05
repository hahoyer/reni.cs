using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    static class SyntaxFactory
    {
        class BracketHandler : DumpableObject, ISyntaxFactory
        {
            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target)
            {
                Tracer.Assert(target.Left != null);
                Tracer.Assert(target.Right == null);
                Tracer.Assert(target.Left.Left == null);
                Tracer.Assert(target.Left.TokenClass.IsBelongingTo(target.TokenClass),
                    $"target.Left.TokenClass: {target.Left.TokenClass}  <===> target.TokenClass: {target.TokenClass}"
                );
                Tracer.Assert(target.Left.Right != null);

                var result = GetSyntax(target.Left.Right).ToBracketFrame(target);

                NotImplementedFunction(target, nameof(target.TokenClass), target.TokenClass);
                return default;
            }
        }

        class DeclarationMarkHandler : DumpableObject, ISyntaxFactory
        {
            Result<Syntax> ISyntaxFactory.GetSyntax(BinaryTree target)
            {
                NotImplementedFunction(target, nameof(target.TokenClass), target.TokenClass);
                return default;
            }
        }

        internal static readonly ISyntaxFactory Bracket = new BracketHandler();
        internal static readonly ISyntaxFactory DeclarationMark = new DeclarationMarkHandler();

        internal static Result<ValueSyntax> GetFrameSyntax(this BinaryTree target)
            => GetSyntax(target).ToFrame();

        static Result<Syntax> GetSyntax(this BinaryTree target)
        {
            if(target.TokenClass is ISyntaxFactoryToken token)
                return token.Provider.GetSyntax(target);

            Dumpable.NotImplementedFunction(target, nameof(target.TokenClass), target.TokenClass);
            return default;
        }

        static Result<ValueSyntax> ToFrame(this Result<Syntax> target)
        {
            Dumpable.NotImplementedFunction(target);
            return default;
        }

        static Result<ValueSyntax> ToBracketFrame(this Result<Syntax> target, BinaryTree root)
        {
            Dumpable.NotImplementedFunction(target, root.TokenClass);
            return default;
        }
    }

    interface ISyntaxFactory
    {
        Result<Syntax> GetSyntax(BinaryTree target);
    }

    interface ISyntaxFactoryToken
    {
        ISyntaxFactory Provider { get; }
    }
}