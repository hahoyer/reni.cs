using System.Collections.Generic;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Parser.TokenClass.Name;
using Reni.Struct;

namespace Reni.Parser
{
    internal static class MainTokenFactory
    {
        internal static TokenFactory Instance { get { return new TokenFactory(CreateMainTokenClasses(), CreateMainPrioTable()); } }

        private static PrioTable CreateMainPrioTable()
        {
            var x = PrioTable.Left("<common>");
            x += PrioTable.Left(
                "at", "content", "_A_T_", "_N_E_X_T_",
                "raw_convert", "construct", "bit_cast", "bit_expand",
                "stable_ref", "consider_as",
                "size",
                "bit_address", "bit_align"
                );

            x += PrioTable.Left("<<", "<*");

            x += PrioTable.Left("~");
            x += PrioTable.Left("&");
            x += PrioTable.Left("|");

            x += PrioTable.Left("*", "/", "\\");
            x += PrioTable.Left("+", "-");

            x += PrioTable.Left("<", ">", "<=", ">=");
            x += PrioTable.Left("=", "<>");

            x += PrioTable.Left("!~");
            x += PrioTable.Left("!&!");
            x += PrioTable.Left("!|!");

            x += PrioTable.Right(":=", "prototype", ":+", ":-", ":*", ":/", ":\\");

            x = x.Level
                (new[]
                     {
                         "+--",
                         "+?+",
                         "?-+"
                     },
                 new[] { "then" },
                 new[] { "else" }
                );
            x += PrioTable.Right("!");
            x += PrioTable.Right(":", "/\\");
            x += PrioTable.Right(",");
            x += PrioTable.Right(";");
            x = x.Level
                (new[]
                     {
                         "++-",
                         "+?-",
                         "?--"
                     },
                 new[] { "(", "[", "{", "<frame>" },
                 new[] { ")", "]", "}", "<end>" }
                );
            //x.Correct("(", "<common>", '-');
            //x.Correct("[", "<common>", '-');
            //x.Correct("{", "<common>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }

        /// <summary>
        /// Creates the main token classes.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, TokenClassBase> CreateMainTokenClasses()
        {
            var result =
                new Dictionary<string, TokenClassBase>
                    {
                        {"\\|/", new ContextOperator()},
                        {":", new Colon()},
                        {":=", new Assignment()},
                        {"=", new Equal()},
                        {">", new CompareOperator()},
                        {">=", new CompareOperator()},
                        {"<", new CompareOperator()},
                        {"<=", new CompareOperator()},
                        {"<>", new NotEqual()},
                        {"<<", new ConcatArrays()},
                        {"<*", new ConcatArrayWithObject()},
                        {"-", new Sign()},
                        {"!", new Exclamation()},
                        {"+", new Sign()},
                        {"/", new Slash()},
                        {"/\\", new TokenClass.Name.Function()},
                        {"*", new Star()},

                        {"_A_T_", new AtToken()},
                        {"arg", new TargT()},
                        {"dump_print", new Feature.DumpPrint.Token()},
                        {"else", new TelseT()},
                        {"enable_cut", new EnableCut()},
                        {"then", new TthenT()},
                        {"this", new TthisT()},
                        {"type", new TypeOperator()}
                    };
            return result;
        }
    }
}