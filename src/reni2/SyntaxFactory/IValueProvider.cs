using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IValueProvider
    {
        Result<ValueSyntax> Get
        (
            BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory
            , FrameItemContainer brackets
        );

        UsageTree GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory);
    }

    class UsageTree
    {
        internal bool Left;
        internal bool Right;
    }

}