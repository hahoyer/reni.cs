using Bnf.Contexts;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class UserSymbol : Form, Define.IDestination, IExpression
    {
        [EnableDump]
        readonly string Name;

        public UserSymbol(Syntax parent, string name)
            : base(parent) => Name = name;

        string Define.IDestination.Name => Name;

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}