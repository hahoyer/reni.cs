using Bnf.Contexts;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    class Or : Form, IExpression, IListForm<IExpression>
    {
        [EnableDump]
        readonly IExpression[] Data;

        public Or(Syntax parent, IExpression[] data)
            : base(parent) => Data = data;

        IExpression[] IListForm<IExpression>.Data => Data;

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}