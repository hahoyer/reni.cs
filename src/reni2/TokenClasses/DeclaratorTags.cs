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
        readonly DeclaratorTags DeclaratorTags;

        internal DeclaratorSyntax(Definable definable, DeclaratorTags declaratorTags)
        {
            Definable = definable;
            DeclaratorTags = declaratorTags;
        }

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren
        {
            get { yield return DeclaratorTags; }
        }
    }
}