using System.Collections.Generic;
using Bnf.Contexts;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Repeat : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Repeat(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
        int? IExpression.Match(SourcePosn sourcePosn, ScannerContext scannerContext)
        {
            NotImplementedMethod(sourcePosn, "statements");
            return null;
        }
    }
}