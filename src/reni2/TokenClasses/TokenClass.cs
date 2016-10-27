using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TokenClass : CommonTokenType<Syntax>, ITokenClass
    {
        string ITokenClass.Id => Id;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);

        [DisableDump]
        internal virtual bool IsVisible => true;
    }
}