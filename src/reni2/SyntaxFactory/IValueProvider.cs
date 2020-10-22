using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IValueProvider
    {
        Result<ValueSyntax> Get(BinaryTree target, Factory factory, FrameItemContainer frameItems);
    }

}