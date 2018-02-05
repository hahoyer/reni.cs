using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    sealed class Reassign : Form, IStatement
    {
        internal interface IDestination : IForm {}

        public readonly IDestination Destination;
        public readonly IExpression Source;

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

            NotImplementedMethod(context);
            return null;
        }
    }
}