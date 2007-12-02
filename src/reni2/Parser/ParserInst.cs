using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Syntax;

namespace Reni.Parser
{
    /// <summary>
    /// The parser singleton
    /// </summary>
    sealed internal class ParserInst
    {
        private PrioTable _prio = StandardPrio();
        private ParserLibrary _parserLibrary = StandardParserLibrary();

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
        public Base Compile(Source source)
        {
            SourcePosn sp = new SourcePosn(source, 0);
            Base start = null;
            PushedSyntaxStack stack = new PushedSyntaxStack();
            stack.Push(new PushedSyntax(start, sp.CreateStart()));
            while (Apply(stack, ref start, ParserLibrary.CreateToken(sp)))
                start = null;
            return PullAndCall(stack, null);
        }

        private bool Apply(PushedSyntaxStack stack, ref Base o, Token token)
        {
            while (true)
            {
                char PrioRel = PrioTable.Op(token, stack.Peek().Token);
                if (PrioRel != '+')
                    o = PullAndCall(stack, o);

                if (PrioRel != '-')
                {
                    stack.Push(new PushedSyntax(o, token));
                    return !token.TokenClass.IsEnd;
                }
                ;
            }
        }

        private static Base PullAndCall(PushedSyntaxStack stack, Base Args)
        {
            PushedSyntax x = stack.Pop();
            return x.CreateSyntax(Args);
        }


        private static ParserLibrary StandardParserLibrary()
        {
            return new ParserLibrary();
        }

        private static PrioTable StandardPrio()
        {
            PrioTable x = PrioTable.LeftAssoc("<else>", "arg");
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
                (new string[]
                     {
                         "+-+",
                         "+?+",
                         "?--"
                     },
                 new string[] {"then"},
                 new string[] {"else"}
                );
            x += PrioTable.RightAssoc(":", "function", "property", "inherit", "apply_operator", "constructor", "converter");
            x += PrioTable.RightAssoc(",");
            x += PrioTable.RightAssoc(";");
            x = x.ParLevel
                (new string[]
                     {
                         "++-",
                         "+?-",
                         "?--"
                     },
                 new string[] {"(", "[", "{", "<frame>"},
                 new string[] {")", "]", "}", "<end>"}
                );
            //x.Correct("(", "<else>", '-');
            //x.Correct("[", "<else>", '-');
            //x.Correct("{", "<else>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }
    }
}