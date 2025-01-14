using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

interface IValueProvider
{
    ValueSyntax? Get(BinaryTree? target, Factory factory, Anchor frameItems);
}