using System.Collections.Generic;
using System.Linq;
using Bnf.Parser;
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
            => Data.Parse(source, context) ??
               context.Repeat(Enumerable.Empty<T>());

        IEnumerable<IExpression> IExpression.Children {get {yield return Data;}}
    }
}