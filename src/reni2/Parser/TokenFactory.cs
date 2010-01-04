using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    internal sealed class TokenFactory : ReniObject, ITokenFactory
    {
        private readonly PrioTable _prioTable;
        private readonly Dictionary<string, TokenClassBase> _tokenClasses;

        internal TokenFactory(Dictionary<string, TokenClassBase> tokenClasses, PrioTable prioTable)
        {
            _tokenClasses = tokenClasses;
            foreach(var tokenClassBase in _tokenClasses)
                tokenClassBase.Value.Name = tokenClassBase.Key;
            
            _prioTable = prioTable;
        }

        Token ITokenFactory.CreateToken(SourcePosn sourcePosn, int length) { return new Token(sourcePosn, length, Find(sourcePosn.SubString(0, length))); }

        private TokenClassBase Find(string name)
        {
            TokenClassBase result;
            if(_tokenClasses.TryGetValue(name, out result))
                return result;
            return UserSymbol.Instance(name);
        }

        internal char Relation(string newTokenName, string recentTokenName) { return _prioTable.Relation(newTokenName, recentTokenName); }
    }
}