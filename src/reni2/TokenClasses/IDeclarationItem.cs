using System;

namespace Reni.TokenClasses
{
    [Obsolete("",true)]
    interface IDeclarationItem
    {
        bool IsDeclarationPart(Syntax syntax);
    }
}