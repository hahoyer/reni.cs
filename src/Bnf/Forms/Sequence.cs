using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Sequence : Form, IExpression, IListForm<IExpression>
    {
        static int NextObjectId;

        [EnableDump]
        readonly IExpression[] Data;

        public Sequence(Syntax parent, IExpression[] data)
            : base(parent, NextObjectId++) => Data = data;

        OccurenceDictionary<T> IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context)
        {
            return context.CreateSequnce(Data);
        }

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            bool trace = ObjectId == -381;
            StartMethodDump(trace, cursor, context);
            BreakExecution();
            try
            {
                var current = cursor;
                var data = new List<T>();
                foreach(var expression in Data)
                {
                    var result = expression.Parse(current, context);
                    if(result == null)
                        return ReturnMethodDump<T>(null);
                    data.Add(result);
                    current = current.Add(result.Value);
                }

                return ReturnMethodDump(context.Sequence(data));
            }
            finally
            {
                EndMethodDump();
            }
        }

        IEnumerable<IExpression> IExpression.Children => Data;

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
        {
            var current = sourcePosn + 0;
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
    }
}