using System.Collections.Generic;
using Bnf.Parser;
using Bnf.StructuredText;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Literal : Form, IExpression
    {
        static string Parse(string name)
        {
            var result = "";
            for(var i = 0; i < name.Length; i++)
                if(name[i] == '\\')
                {
                    i++;
                    switch(name[i])
                    {
                        case 'n':
                            result += "\n";
                            break;
                        case 'r':
                            result += "\r";
                            break;
                        case 't':
                            result += "\t";
                            break;
                        case '\\':
                            result += "\\";
                            break;
                        default:
                            Tracer.Assert(false);
                            break;
                    }
                }
                else
                    result += name[i];

            return result;
        }

        [EnableDump]
        readonly string Text;

        public Literal(Syntax parent, string name)
            : base(parent) => Text = Parse(name);

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => sourcePosn.StartsWith(Text) ? (int?) Text.Length : null;

        T IExpression.Parse<T>(IParserCursor source, IContext<T> context)
        {
            var token = context[source];
            if(token.Characters.Id != Text)
                return null;
            
            NotImplementedMethod(source, nameof(context), nameof(token), token);
            return null;
        }

        IEnumerable<ITerminal> IExpression.Terminals
        {
            get
            {
                yield break;
            }
        }

    }
}