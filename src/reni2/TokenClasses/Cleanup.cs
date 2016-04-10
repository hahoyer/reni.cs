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

        protected override Result<OldSyntax> OldTerminal(SourcePart token)
            => Result<OldSyntax>.From(new EmptyList(token).ToCompound);

        protected override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => new EmptyList(token).Cleanup(token, right);

        protected override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => Result<OldSyntax>.From(left.ToCompound);

        protected override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => left.Cleanup(token, right);
    }
}