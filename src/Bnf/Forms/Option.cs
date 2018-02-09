using Bnf.Contexts;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Option : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Option(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => Data.Match(sourcePosn, scannerContext) ?? 0;

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}