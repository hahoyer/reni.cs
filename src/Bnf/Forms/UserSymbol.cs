using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class UserSymbol : Form, Define.IDestination, IExpression
    {
        [EnableDump]
        readonly string Name;

        public UserSymbol(Syntax parent, string name)
            : base(parent) => Name = name;

        protected override Result GetResult(Context context)
            => context.UserSymbol(Parent.Token.Characters, Name);
    }
}