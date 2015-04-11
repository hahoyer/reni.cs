using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Formatting;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class EndToken : NonPrefixToken
    {
        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => Checked<Syntax>.From(left.ToCompiledSyntax);

        protected override ITreeItemFactory TreeItemFactory => Main.FactoryInstance;

        protected override Checked<Syntax> Terminal(SourcePart token) => new EmptyList();
        public override string Id => PrioTable.EndOfText;

    }
}