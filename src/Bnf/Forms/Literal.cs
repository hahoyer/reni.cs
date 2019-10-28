using System.Collections.Generic;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Literal : Form, IExpression, ILiteralContainer
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
        internal readonly ILiteral Value;

        public Literal(Syntax parent, ParserLiteral value)
            : base(parent) => Value = value;

        int? IExpression.Match(SourcePosn sourcePosn, IScannerContext scannerContext)
            => sourcePosn.StartsWith(Text) ? (int?) Text.Length : null;

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            var token = context[cursor];
            return token.Characters.Id == Text ? context.LiteralMatch(token) : null;
        }

        OccurenceDictionary<T> IExpression.GetTokenOccurences<T>(Base.IContext<T> context)
            => context.CreateOccurence(this);

        IEnumerable<IExpression> IExpression.Children {get {yield break;}}
        ILiteral ILiteralContainer.Value => Value;

        string Text => Value.Value;
    }
}