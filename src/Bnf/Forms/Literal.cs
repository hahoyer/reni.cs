using System.Collections.Generic;
using Bnf.Base;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Bnf.Forms
{
    sealed class Literal : Form, IExpression, ILiteral
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

        T IExpression.Parse<T>(IParserCursor cursor, IContext<T> context)
        {
            var token = context[cursor];
            return token.Characters.Id == Text ? context.LiteralMatch(token) : null;
        }

        OccurenceDictionary<T> IExpression.GetTokenOccurences<T>(Definitions<T>.IContext context) 
            => context.CreateOccurence(this);

        IEnumerable<IExpression> IExpression.Children {get {yield break;}}
        string IUniqueIdProvider.Value => Text;
    }
}