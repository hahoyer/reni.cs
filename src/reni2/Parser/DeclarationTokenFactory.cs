using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class DeclarationTokenFactory : GenericTokenFactory<Syntax>
    {
        public DeclarationTokenFactory(string title)
            : base(title)
        {
        }

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
            => new InvalidDeclarationError(name);
    }

    sealed class InvalidDeclarationError : DumpableObject, IParserTokenType<Syntax>, ITokenClass
    {
        readonly string Name;
        public InvalidDeclarationError(string name) => Name = name;

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            => Syntax.Create(left, this, token, right);

        string IParserTokenType<Syntax>.PrioTableId => Name;
        string ITokenClass.Id => Name;
    }
}