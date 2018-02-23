using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
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

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
            => Data.Parse(cursor, context) ??
               context.Repeat(Enumerable.Empty<T>());

        OccurenceDictionary<T>  IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }

        IEnumerable<IExpression> IExpression.Children {get {yield return Data;}}
    }
}