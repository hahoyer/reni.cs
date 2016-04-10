using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class DefinableSyntax : OldSyntax
    {
        internal DefinableSyntax
            (SourcePart token, Definable definable)
        {
            Definable = definable;
            Token = token;
            StopByObjectIds();
        }

        internal Definable Definable { get; }
        internal override SourcePart Token { get; }

        internal override Checked<OldSyntax> RightSyntax(OldSyntax right, SourcePart token)
            => Checked<OldSyntax>.From(ExpressionSyntax.OldCreate(null, Definable, right, token));

        [DisableDump]
        internal override Checked<Value> ToCompiledSyntax
            => Checked<Value>
                .From(ExpressionSyntax.Create(null, Definable, null, Token));
    }
}