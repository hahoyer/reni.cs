using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    interface IItem
    {
        Anchor Anchor { get; }
        Syntax[] DirectChildren { get; }
        BinaryTree SpecialAnchor { get; }
    }
}