using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    internal sealed class MainTokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        protected override PrioTable GetPrioTable()
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
                 new[] {"then"},
                 new[] {"else"}
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
                 new[] {"(", "[", "{", "<frame>"},
                 new[] {")", "]", "}", "<end>"}
                );
            //x.Correct("(", "<common>", '-');
            //x.Correct("[", "<common>", '-');
            //x.Correct("{", "<common>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }

        /// <summary>
        ///     Creates the main token classes.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            return new Dictionary<string, TokenClasses.TokenClass>
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
                       {"/\\", new TokenClasses.Function()},
                       {"*", new Star()},
                       {"_A_T_", new AtToken()},
                       {"arg", new ArgToken()},
                       {"dump_print", new Feature.DumpPrint.Token()},
                       {"else", new ElseToken()},
                       {"enable_cut", new EnableCut()},
                       {"then", new ThenToken()},
                       {"this", new ThisToken()},
                       {"type", new TypeOperator()}
                   };
        }

        protected override TokenClasses.TokenClass GetListClass() { return new List(); }
        protected override TokenClasses.TokenClass GetRightParenthesisClass(int level) { return new RightParenthesis(level); }
        protected override TokenClasses.TokenClass GetLeftParenthesisClass(int level) { return new LeftParenthesis(level); }
        protected override TokenClasses.TokenClass GetNumberClass() { return new Number(); }
        protected override TokenClasses.TokenClass NewTokenClass(string name) { return new UserSymbol(name); }
    }
}