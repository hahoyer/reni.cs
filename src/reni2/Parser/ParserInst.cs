using System.Collections.Generic;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Syntax;

namespace Reni.Parser
{
    /// <summary>
    /// The parser singleton
    /// </summary>
    internal sealed class ParserInst
    {
        private readonly ParserLibrary _parserLibrary = StandardParserLibrary();
        private readonly PrioTable _prio = StandardPrio();

        /// <summary>
        /// The priority table to use
        /// </summary>
        [Node]
        public PrioTable PrioTable { get { return _prio; } }

        /// <summary>
        /// The parser library to use
        /// </summary>
        public ParserLibrary ParserLibrary { get { return _parserLibrary; } }

        /// <summary>
        /// Scans and parses source and creates the syntax tree
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public SyntaxBase Compile(Source source)
        {
            var sp = new SourcePosn(source, 0);
            SyntaxBase start = null;
            var stack = new Stack<PushedSyntax>();
            stack.Push(new PushedSyntax(start, sp.CreateStart()));
            while(Apply(stack, ref start, ParserLibrary.CreateToken(sp)))
                start = null;
            return PullAndCall(stack, null);
        }

        private bool Apply(Stack<PushedSyntax> stack, ref SyntaxBase o, Token token)
        {
            while(true)
            {
                var PrioRel = PrioTable.Op(token, stack.Peek().Token);
                if(PrioRel != '+')
                    o = PullAndCall(stack, o);

                if(PrioRel != '-')
                {
                    stack.Push(new PushedSyntax(o, token));
                    return !token.TokenClass.IsEnd;
                }
            }
        }

        private static SyntaxBase PullAndCall(Stack<PushedSyntax> stack, SyntaxBase Args)
        {
            var x = stack.Pop();
            return x.CreateSyntax(Args);
        }

        private static ParserLibrary StandardParserLibrary()
        {
            return new ParserLibrary();
        }

        private static PrioTable StandardPrio()
        {
            var x = PrioTable.LeftAssoc("<else>", "arg");
            x += PrioTable.LeftAssoc(
                "array", "explicit_ref",
                "at", "content", "_A_T_", "_N_E_X_T_",
                "raw_convert", "construct", "bit_cast", "bit_expand",
                "stable_ref", "consider_as",
                "size",
                "bit_address", "bit_align"
                );

            x += PrioTable.LeftAssoc(".");

            x += PrioTable.LeftAssoc("~");
            x += PrioTable.LeftAssoc("&");
            x += PrioTable.LeftAssoc("|");

            x += PrioTable.LeftAssoc("*", "/", "\\");
            x += PrioTable.LeftAssoc("+", "-");

            x += PrioTable.LeftAssoc("<", ">", "<=", ">=");
            x += PrioTable.LeftAssoc("=", "<>");

            x += PrioTable.LeftAssoc("not");
            x += PrioTable.LeftAssoc("and");
            x += PrioTable.LeftAssoc("or");

            x += PrioTable.RightAssoc(":=", "prototype", ":+", ":-", ":*", ":/", ":\\");

            x = x.ParLevel
                (new[]
                {
                    "+-+",
                    "+?+",
                    "?--"
                },
                    new[] {"then"},
                    new[] {"else"}
                );
            x += PrioTable.RightAssoc(":", "function", "property", "inherit", "apply_operator", "constructor",
                "converter");
            x += PrioTable.RightAssoc(",");
            x += PrioTable.RightAssoc(";");
            x = x.ParLevel
                (new[]
                {
                    "++-",
                    "+?-",
                    "?--"
                },
                    new[] {"(", "[", "{", "<frame>"},
                    new[] {")", "]", "}", "<end>"}
                );
            //x.Correct("(", "<else>", '-');
            //x.Correct("[", "<else>", '-');
            //x.Correct("{", "<else>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }
    }
}