using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClasses;
using Reni.ReniParser.TokenClasses;
using Reni.Sequence;
using Reni.Struct;

namespace Reni.ReniParser
{
    internal sealed class MainTokenFactory : TokenFactory<TokenClasses.TokenClass>
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
                       {"arg", new TargT()},
                       {"dump_print", new Feature.DumpPrint.Token()},
                       {"else", new ElseToken()},
                       {"enable_cut", new EnableCut()},
                       {"then", new ThenToken()},
                       {"this", new TthisT()},
                       {"type", new TypeOperator()}
                   };
        }

        protected override TokenClasses.TokenClass GetListClass() { return new List(); }
        protected override TokenClasses.TokenClass RightParentethesisClass(int level) { return new RPar(level); }
        protected override TokenClasses.TokenClass LeftParentethesisClass(int level) { return new LPar(level); }

        protected override TokenClasses.TokenClass NewTokenClass(string name) { return UserSymbol.Instance(name); }
    }
}