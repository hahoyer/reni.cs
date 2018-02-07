using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Option : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Option(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}