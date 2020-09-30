using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class MainTokenFactory : GenericTokenFactory<Syntax>
    {
        readonly Compiler<Syntax>.Component Declaration;
        readonly List<UserSymbol> UserSymbols = new List<UserSymbol>();

        public MainTokenFactory(Compiler<Syntax>.Component declaration, string title)
            : base(title)
        {
            Declaration = declaration;
        }

        protected override IParserTokenType<Syntax> SpecialTokenClass(System.Type type)
        {
            if(type == typeof(Exclamation))
                return new Exclamation(Declaration.SubParser);

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
    }
}