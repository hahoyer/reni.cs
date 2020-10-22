using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class FunctionHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
        (
            BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory
            , FrameItemContainer brackets
        )
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
                        , target, null)
                );
        }

    }
}