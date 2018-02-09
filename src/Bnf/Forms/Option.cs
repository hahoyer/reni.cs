using System.Collections.Generic;
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

        int? IExpression.Match(SourcePosn sourcePosn, ScannerContext scannerContext)
        {
            NotImplementedMethod(sourcePosn, "statements");
            return null;
        }

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}