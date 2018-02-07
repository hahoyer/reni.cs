using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Reassign : Form, IStatement
    {
        internal interface IDestination : IForm {}

        [EnableDump]
        readonly IDestination Destination;

        [EnableDump]
        readonly IExpression Source;

        public Reassign(Syntax parent, IDestination destination, IExpression source)
            : base(parent)
        {
            Destination = destination;
            Source = source;
        }


        protected override Result GetResult(Context context)
        {
            var source = Source.GetResult(context.ReassignValue);
            var destination = Destination.GetResult(context.ReassignDestination(source.DataType));

            return destination.Reassign(Parent.Token.Characters, source);
        }
    }
}