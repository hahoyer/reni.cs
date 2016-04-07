using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        protected override Checked<OldSyntax> OldTerminal(SourcePart token)
            => Checked<OldSyntax>.From(new EmptyList(token).ToCompound);

        protected override Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => new EmptyList(token).Cleanup(token, right);

        protected override Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => Checked<OldSyntax>.From(left.ToCompound);

        protected override Checked<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => left.Cleanup(token, right);
    }
}