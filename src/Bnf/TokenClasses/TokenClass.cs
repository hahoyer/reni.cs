using Bnf.Forms;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.TokenClasses
{
    interface ITokenType
    {
        string Id {get;}
        IForm GetForm(Syntax parent);
    }

    abstract class TokenType : PriorityParserTokenType<Syntax>, ITokenType
    {
        string ITokenType.Id => Id;

        IForm ITokenType.GetForm(Syntax parent) => GetForm(parent);

        string Name => Id;

        protected abstract IForm GetForm(Syntax parent);

        protected override string GetNodeDump() => GetType().PrettyName() + "(" + Name.Quote() + ")";

        protected sealed override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.Create(left, this, token, right);
    }
}