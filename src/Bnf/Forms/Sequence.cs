using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms {
    class Sequence : Form, IExpression, IListForm<IExpression>
    {
        [EnableDump]
        readonly IExpression[] Data;

        public Sequence(Syntax parent, IExpression[] data)
            : base(parent) => Data = data;

        IExpression[] IListForm<IExpression>.Data => Data;

        protected override Result GetResult(Context context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}