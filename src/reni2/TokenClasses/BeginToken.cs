using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class BeginToken : NonSuffixToken
    {
        public override string Id => PrioTable.BeginOfText;

        protected override Checked<Syntax> Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;

        }

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right) => right;
    }
}