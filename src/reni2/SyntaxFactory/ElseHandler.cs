using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ElseHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory)
            => (
                    factory.GetValueSyntax(target.Left?.Left),
                    factory.GetValueSyntax(target.Left?.Right),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply((condition, thenClause, elseClause)
                    => (ValueSyntax)new CondSyntax(condition, thenClause, elseClause, target));
    }
}