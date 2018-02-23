using System.Collections.Generic;
using Bnf.Base;
using Bnf.Parser;
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

        OccurenceDictionary<T> IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context)
        {
            var children = Data.GetTokenOccurences(context);
            return context.CreateRepeat(children);
        }

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            var data = new List<T>();
            while(true)
            {
                var result = Data.Parse(cursor, context);
                if(result == null)
                    return context.Repeat(data);
                data.Add(result);
            }
        }

        IEnumerable<IExpression> IExpression.Children {get {yield return Data;}}

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