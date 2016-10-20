using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class AnyTokenType : PredefinedTokenFactory<Syntax>
    {
        readonly TokenFactory Parent;
        public AnyTokenType(TokenFactory parent) { Parent = parent; }

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
            => Parent.GetTokenClass(name);

        protected override IEnumerable<IParserTokenType<Syntax>> GetPredefinedTokenClasses()
            => Parent.PredefinedTokenClasses;
    }
}