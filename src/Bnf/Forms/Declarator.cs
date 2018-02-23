using System.Collections.Generic;
using Bnf.Base;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Declarator : Form, Define.IDestination, IExpression
    {
        [EnableDump]
        readonly string Name;

        public Declarator(Syntax parent, string name)
            : base(parent) => Name = name;

        string Define.IDestination.Name => Name;

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            var name = Name;
            var trace = name == "array_variable" && cursor.Position > 0;
            StartMethodDump(trace, cursor, context);
            try
            {
                BreakExecution();
                var declaration = context[name];
                Dump(nameof(declaration), declaration);
                var localCursor = cursor.TryDeclaration(name);
                if(localCursor == null)
                    return ReturnMethodDump<T>(null);
                var result = declaration.Expression.Parse(localCursor, context);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        OccurenceDictionary<T>  IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context) 
            => context.CreateOccurence(this);

        IEnumerable<IExpression> IExpression.Children {get {yield break;}}

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => scannerContext.Resolve(Name).Function(sourcePosn);

        T ResultFunction<T>()
            where T : class, IParseSpan, ISourcePartProxy
        {
            NotImplementedMethod();
            return null;
        }
    }
}