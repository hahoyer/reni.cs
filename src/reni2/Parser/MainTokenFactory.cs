using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class MainTokenFactory : GenericTokenFactory<Syntax>, Compiler<Syntax>.IComponent
    {
        readonly List<UserSymbol> UserSymbols = new List<UserSymbol>();
        ISubParser<Syntax> DeclarationSubParser;

        protected override IParserTokenType<Syntax> SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(DeclarationSubParser);

            return base.SpecialTokenClass(type);
        }

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
        {
            var result = new UserSymbol(name);
            UserSymbols.Add(result);
            return result;
        }

        [DisableDump]
        internal IEnumerable<IParserTokenType<Syntax>> AllTokenClasses
            => PredefinedTokenClasses.Concat(UserSymbols);

        Compiler<Syntax>.Component Compiler<Syntax>.IComponent.Current
        {
            set { DeclarationSubParser = value.Parent["Declaration"].SubParser; }
        }
    }
}