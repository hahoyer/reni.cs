using System.Collections.Generic;
using System.Linq;
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

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
        {
            StartMethodDump(false, source, context);

            try
            {
                foreach(var expression in Data)
                {
                    Dump(nameof(expression), expression);
                    var result = expression.Parse(source, context);
                    Dump(nameof(result), result);
                    if(result != null)
                        return ReturnMethodDump(result);
                }

                return ReturnMethodDump<T>(null);
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