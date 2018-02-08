using Bnf.Contexts;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Literal : Form, IExpression
    {
        [EnableDump]
        readonly string Name;

        public Literal(Syntax parent, string name)
            : base(parent) => Name = name;

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}