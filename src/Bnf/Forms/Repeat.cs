using System.Collections.Generic;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Repeat : Form, IExpression
    {
        [EnableDump]
        readonly IExpression Data;

        public Repeat(Syntax parent, IExpression data)
            : base(parent) => Data = data;

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
        {
            var data = new List<T>();
            while(true)
            {
                var result = Data.Parse(source, context);
                if(result == null)
                    return context.Repeat(data);
                data.Add(result);
            }
        }

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
        {
            var current = sourcePosn + 0;
            while(true)
            {
                var result = Data.Match(current, scannerContext);
                if(result == null)
                    return current - sourcePosn;
                current += result.Value;
            }
        }
    }
}