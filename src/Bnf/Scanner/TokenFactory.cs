using System.Collections.Generic;
using System.Linq;
using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Scanner
{
    abstract class TokenFactoryWithUserSymbols<TSourcePart> : GenericTokenFactory<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        readonly List<IParserTokenType<TSourcePart>> UserSymbols = new List<IParserTokenType<TSourcePart>>();

        protected TokenFactoryWithUserSymbols(string title)
            : base(title) { }

        [DisableDump]
        internal IEnumerable<IParserTokenType<TSourcePart>> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);

        protected override IParserTokenType<TSourcePart> GetTokenClass(string name)
        {
            var result = NewSymbol(name);
            UserSymbols.Add(result);
            return result;
        }

        protected abstract IParserTokenType<TSourcePart> NewSymbol(string name);
    }

    sealed class TokenFactory : TokenFactoryWithUserSymbols<Syntax>
    {
        public TokenFactory(string title)
            : base(title) { }

        protected override IParserTokenType<Syntax> NewSymbol(string name) => new UserSymbol(name);
    }
}