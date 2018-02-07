using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass
    {
        public static string TokenId(int level)
            => level == 0 ? PrioTable.BeginOfText : "{[(".Substring(level - 1, 1);

        public LeftParenthesis(int level) => Level = level;

        [DisableDump]
        internal int Level {get;}

        [DisableDump]
        public override string Id => TokenId(Level);

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }
}