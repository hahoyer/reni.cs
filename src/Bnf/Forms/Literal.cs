using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Literal : Form, IExpression
    {
        [EnableDump]
        readonly string Name;

        public Literal(Syntax parent, string name)
            : base(parent) => Name = name;

        protected override Result GetResult(Context context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}