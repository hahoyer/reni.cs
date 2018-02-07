using Bnf.Forms;
using hw.DebugFormatter;

namespace Bnf.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        public UserSymbol(string name) => Id = name;

        [DisableDump]
        public override string Id {get;}

        protected override IForm GetForm(Syntax parent)
        {
            Tracer.Assert(parent.Left == null);
            Tracer.Assert(parent.Right == null);
            return new Forms.UserSymbol(parent, Id);
        }
    }

}