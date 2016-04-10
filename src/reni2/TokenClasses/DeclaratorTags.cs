using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class DeclaratorSyntax : Value
    {
        [EnableDump]
        readonly Definable Definable;
        [EnableDump]
        readonly IDeclarationTag[] DeclaratorTags;

        internal DeclaratorSyntax(Definable definable, IDeclarationTag[] declaratorTags)
        {
            Definable = definable;
            DeclaratorTags = declaratorTags;
        }
    }
}