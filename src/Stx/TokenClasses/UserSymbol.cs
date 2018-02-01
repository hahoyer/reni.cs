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

            return FormBase.CreateUserSymbol(parent, Id, parent.Right?.Form);

        }
    }
}