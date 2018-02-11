using hw.DebugFormatter;

namespace Bnf.Forms
{
    sealed class Define : Form, IStatement
    {
        internal interface IDestination : IForm
        {
            string Name {get;}
        }

        [EnableDump]
        readonly IDestination Destination;

        [EnableDump]
        readonly IExpression Source;

        public Define(Syntax parent, IDestination destination, IExpression source)
            : base(parent)
        {
            Destination = destination;
            Source = source;
        }

        string IStatement.Name => Destination.Name;
        IExpression IStatement.Value => Source;

        protected override string GetNodeDump() => base.GetNodeDump() + "." + Destination.Name;
    }
}