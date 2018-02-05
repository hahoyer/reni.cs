using hw.DebugFormatter;
using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    sealed class UserSymbol : Form, Reassign.IDestination, IExpression
    {
        [EnableDump]
        readonly string Name;

        public UserSymbol(Syntax parent, string name)
            : base(parent) => Name = name;

        protected override Result GetResult(Context context)
            => context.UserSymbol(Parent.Token.Characters, Name);
    }
}