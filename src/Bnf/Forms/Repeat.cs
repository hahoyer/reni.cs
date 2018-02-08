using Bnf.Contexts;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Repeat : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Repeat(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}