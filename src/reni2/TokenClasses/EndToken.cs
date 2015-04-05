using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class EndToken : NonPrefixToken
    {
        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => Checked<Syntax>.From(left.ToCompiledSyntax);

        protected override Checked<Syntax> Terminal(SourcePart token) => new EmptyList();
        public override string Id => PrioTable.EndOfText;

        internal override string Reformat
            (SourceSyntax target, IFormattingConfiguration configuration)
        {
            if(target.Left != null && target.Right == null)
                return target.Left.Reformat(configuration);

            return base.Reformat(target, configuration);
        }
    }
}