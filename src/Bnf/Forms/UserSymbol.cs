using System.Collections.Generic;
using Bnf.Parser;
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

        IEnumerable<IExpression> IExpression.Children {get {yield break;}}

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => scannerContext.Resolve(Name).Function(sourcePosn);
    }
}