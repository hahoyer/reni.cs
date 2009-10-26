using System;
using System.Collections.Generic;
using System.Reflection;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    [Serializable]
    internal abstract class TokenFactory
    {
        private readonly PrioTable _prioTable;
        private readonly Dictionary<string, TokenClassBase> _tokenClasses;

        protected TokenFactory(Dictionary<string, TokenClassBase> tokenClasses, PrioTable prioTable)
        {
            _tokenClasses = tokenClasses;
            _prioTable = prioTable;
        }

        internal Token CreateToken(SourcePosn sourcePosn, int length)
        {
            return new Token(sourcePosn, length, Find(sourcePosn.SubString(0, length)));
        }

        private TokenClassBase Find(string name)
        {
            TokenClassBase result;
            if(_tokenClasses.TryGetValue(name, out result))
                return result;
            return UserSymbol.Instance(name);
        }

        public char Relation(Token newToken, Token topToken)
        {
            return _prioTable.Relation(newToken, topToken);
        }
    }

    [Serializable]
    internal sealed class TokenFactory<TTokenAttribute> : TokenFactory where TTokenAttribute : TokenAttributeBase, new()
    {
        internal TokenFactory()
            : base(CreateTokenClasses(), new TTokenAttribute().CreatePrioTable()) { }

        private static Dictionary<string, TokenClassBase> CreateTokenClasses()
        {
            var result = new Dictionary<string, TokenClassBase>();
            var assembly = Assembly.GetAssembly(typeof(TTokenAttribute));
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(TTokenAttribute), true);
                foreach (TTokenAttribute attribute in attributes)
                {
                    var instance = (TokenClassBase)Activator.CreateInstance(type, new object[0]);
                    instance.Name = attribute.Token;
                    result.Add(attribute.Token, instance);
                }
            }
            return result;
        }
    }
}