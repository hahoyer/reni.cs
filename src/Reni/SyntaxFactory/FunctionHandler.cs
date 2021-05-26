using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class FunctionHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor frameItems)
        {
            var token = (Function)target.TokenClass;
            return new FunctionSyntax
            (
                factory.GetValueSyntax(target.Left)
                , token.IsImplicit
                , token.IsMetaFunction
                , factory.GetValueSyntax(target.Right)
                , Anchor.Create(target).Combine(frameItems)
            );
        }
    }
}