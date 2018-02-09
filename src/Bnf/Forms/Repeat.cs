using Bnf.Contexts;
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

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
        {
            var current = sourcePosn+0;
            while(true)
            {
                var result = Data.Match(current, scannerContext);
                if(result == null)
                    return current - sourcePosn;
                current += result.Value;
            }
        }

        protected override string GetResult(IContext context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}