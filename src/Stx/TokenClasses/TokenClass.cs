using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using Stx.CodeItems;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;

namespace Stx.TokenClasses
{
    interface ITokenClass
    {
        string Id {get;}
        Result GetResult(Context context, Syntax left, IToken token, Syntax right);
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

        Result ITokenClass.GetResult(Context context, Syntax left, IToken token, Syntax right)
            => GetResult(context, left, token, right);

        string Name => Names.OrderByDescending(i => i.Value).FirstOrDefault().Key ?? Id;

        protected abstract Result GetResult(Context context, Syntax left, IToken token, Syntax right);

        protected override string GetNodeDump() => GetType().PrettyName() + "(" + Name.Quote() + ")";

        protected sealed override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.Create(left, this, token, right);
    }

}