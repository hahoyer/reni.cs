using System;
using hw.Parser;

namespace Stx {
    sealed class UserSymbol : IParserTokenType<Syntax>
    {
        readonly string Name;
        public UserSymbol(string name) {Name = name;}

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right) =>
            throw new NotImplementedException();

        string IParserTokenType<Syntax>.PrioTableId => throw new NotImplementedException();
    }
}