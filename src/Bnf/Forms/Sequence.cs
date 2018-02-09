using System.Collections.Generic;
using Bnf.Contexts;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Sequence : Form, IExpression, IListForm<IExpression>
    {
        [EnableDump]
        readonly IExpression[] Data;

        public Sequence(Syntax parent, IExpression[] data)
            : base(parent) => Data = data;

        int? IExpression.Match(SourcePosn sourcePosn, ScannerContext scannerContext)
        {
            var current = sourcePosn;
            foreach(var expression in Data)
            {
                var result = expression.Match(current, scannerContext);
                if(result == null)
                    return null;
                current += result.Value;
            }

            return current - sourcePosn;
        }

        IExpression[] IListForm<IExpression>.Data => Data;

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}