using Bnf.Contexts;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Option : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Option(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}