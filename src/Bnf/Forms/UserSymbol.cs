using System.Collections.Generic;
using Bnf.Contexts;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class UserSymbol : Form, Define.IDestination, IExpression
    {
        [EnableDump]
        readonly string Name;

        public UserSymbol(Syntax parent, string name)
            : base(parent) => Name = name;

        string Define.IDestination.Name => Name;

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => scannerContext.Resolve(Name).Function(sourcePosn);

        protected override string GetResult(IContext context)
        {
            NotImplementedFunction(context);
            return null;
        }
    }
}