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

        protected override Checked<Syntax> Terminal(SourcePart token)
            => Checked<Syntax>.From(new EmptyList(token).ToCompound);

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => new EmptyList(token).Cleanup(token, right);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => Checked<Syntax>.From(left.ToCompound);

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => left.Cleanup(token, right);
    }
}