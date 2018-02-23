using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
using Bnf.Parser;
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

        OccurenceDictionary<T> IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            StartMethodDump(false, cursor, context);

            try
            {
                var results = Data
                    .Select(expression => expression.Parse(cursor, context))
                    .OrderByDescending(r => r?.Value ?? -1);

                var result = results.FirstOrDefault();

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IEnumerable<IExpression> IExpression.Children => Data;

        IExpression[] IListForm<IExpression>.Data => Data;
    }
}