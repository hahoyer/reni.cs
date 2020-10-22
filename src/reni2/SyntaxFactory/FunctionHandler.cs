using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class FunctionHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get
        (
            BinaryTree target, Factory factory
            , FrameItemContainer frameItems
        )
        {
            var token = (Function)target.TokenClass;
            return new FunctionSyntax
            (
                factory.GetValueSyntax(target.Left)
                , token.IsImplicit
                , token.IsMetaFunction
                , factory.GetValueSyntax(target.Right)
                , target
                , frameItems
            );
        }
    }
}