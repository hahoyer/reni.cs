using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Repeat : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Repeat(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}