using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.Helper;
using hw.Parser;

namespace Bnf.TokenClasses
{
    interface ITokenClass
    {
        string Id {get;}
        IForm GetForm(Syntax parent);
    }

    abstract class TokenClass : ParserTokenType<Syntax>, ITokenClass, IAliasKeeper
    {
        readonly IDictionary<string, int> Names = new Dictionary<string, int>();

        void IAliasKeeper.Add(string value)
        {
            if(!Names.ContainsKey(value))
                Names[value] = 0;

            Names[value]++;
        }

        string ITokenClass.Id => Id;

        IForm ITokenClass.GetForm(Syntax parent) => GetForm(parent);

        string Name => Names.OrderByDescending(i => i.Value).FirstOrDefault().Key ?? Id;

        protected abstract IForm GetForm(Syntax parent);

        protected override string GetNodeDump() => GetType().PrettyName() + "(" + Name.Quote() + ")";

        protected sealed override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.Create(left, this, token, right);
    }
}