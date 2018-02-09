using System.Collections.Generic;
using System.Linq;
using Bnf.Contexts;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Or : Form, IExpression, IListForm<IExpression>
    {
        [EnableDump]
        readonly IExpression[] Data;

        public Or(Syntax parent, IExpression[] data)
            : base(parent) => Data = data;

        int? IExpression.Match(SourcePosn sourcePosn, ScannerContext scannerContext) 
            => Data.Select(e => e.Match(sourcePosn, scannerContext)).FirstOrDefault(i=>i != null);

        IExpression[] IListForm<IExpression>.Data => Data;

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}