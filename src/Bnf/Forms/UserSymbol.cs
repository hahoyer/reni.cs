using Bnf.Parser;
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

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context) 
            => context[Name].Parse(source, context);

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => scannerContext.Resolve(Name).Function(sourcePosn);
    }
}