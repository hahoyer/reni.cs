using hw.DebugFormatter;
using Stx.Forms;

namespace Stx.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        public UserSymbol(string name) => Id = name;

        [DisableDump]
        public override string Id {get;}

        protected override IForm GetForm(Syntax parent)
        {
            Tracer.Assert(parent.Left == null, () => parent.Left.Dump());

            string name = Id;
            IForm index = parent.Right?.Form;
            return index == null
                ? (IForm) new UserSymbolForm(parent, name)
                : new UserSymbolFormWithIndex(parent, name, index);

        }
    }
}