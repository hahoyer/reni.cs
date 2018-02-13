using Bnf.Base;
using Bnf.Parser;
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

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
        {
            NotImplementedMethod(source, nameof(context));
            return null;
        }
    }
}