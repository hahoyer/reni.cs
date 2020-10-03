using System;

namespace Reni.TokenClasses
{
    [Obsolete("",true)]
    interface IDeclarationItem
    {
        bool IsDeclarationPart(BinaryTree binaryTree);
    }
}