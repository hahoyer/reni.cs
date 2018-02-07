using System.Collections.Generic;
using System.Linq;
using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.Scanner
{
    sealed class TokenFactory : GenericTokenFactory<Syntax>
    {
        readonly List<UserSymbol> UserSymbols = new List<UserSymbol>();

        public TokenFactory(string title)
            : base(title) {}

        [DisableDump]
        internal IEnumerable<IParserTokenType<Syntax>> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);

        protected override string GetTokenClassKeyFromToken(string id) => id.ToLower();

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
        {
            var result = new UserSymbol(name);
            UserSymbols.Add(result);
            return result;
        }
    }
}