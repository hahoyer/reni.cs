using System.Collections.Generic;
using Reni.Parser.TokenClass;

namespace Reni.Parser
{
    internal static class DeclarationTokenFactory
    {
        internal static TokenFactory Instance { get { return new TokenFactory(CreateDeclarationTokenClasses(), CreateDeclarationPrioTable()); } }

        private static PrioTable CreateDeclarationPrioTable()
        {
            var x = PrioTable.Left("!");
            x += PrioTable.Left("property", "converter");
            x = x.Level
                (new[]
                     {
                         "++-",
                         "+?-",
                         "?--"
                     },
                 new[] { "(", "[", "{", "<frame>" },
                 new[] { ")", "]", "}", "<end>" }
                );
            x += PrioTable.Left("<common>");
            return x;
        }

        private static Dictionary<string, TokenClassBase> CreateDeclarationTokenClasses()
        {
            var result =
                new Dictionary<string, TokenClassBase>
                    {
                        {"converter", new Converter()},
                        {"property", new Property()}
                    };
            return result;
        }
    }
}