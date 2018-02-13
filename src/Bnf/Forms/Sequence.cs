using System.Collections.Generic;
using Bnf.Parser;
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

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
        {
            var current = source.Clone;
            var data = new List<T>();
            foreach(var expression in Data)
            {
                var result = expression.Parse(current, context);
                if(result == null)
                    return null;
                data.Add(result);
                current.Add(result.Value);
            }

            return context.Sequence(data);
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