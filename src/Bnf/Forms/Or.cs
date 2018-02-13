using System.Linq;
using Bnf.Parser;
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

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => Data.Select(e => e.Match(sourcePosn, scannerContext)).FirstOrDefault(i => i != null);

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
            => Data.Select(e => e.Parse(source, context)).FirstOrDefault(i => i != null);

        IExpression[] IListForm<IExpression>.Data => Data;
    }
}