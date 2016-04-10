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

        internal override Result<OldSyntax> RightSyntax(OldSyntax right, SourcePart token)
            => Result<OldSyntax>.From(ExpressionSyntax.OldCreate(null, Definable, right, token));

        [DisableDump]
        internal override Result<Value> ToCompiledSyntax
            => Result<Value>
                .From(ExpressionSyntax.Create(null, Definable, null, Token));
    }
}