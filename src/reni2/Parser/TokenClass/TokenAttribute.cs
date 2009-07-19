using HWClassLibrary.Debug;
using System;
using JetBrains.Annotations;

namespace Reni.Parser.TokenClass
{
    [MeansImplicitUse]
    internal sealed class TokenAttribute : TokenAttributeBase
    {
        internal override PrioTable CreatePrioTable()
        {
            var x = PrioTable.LeftAssoc("<common>");
            x += PrioTable.LeftAssoc(
                "at", "content", "_A_T_", "_N_E_X_T_",
                "raw_convert", "construct", "bit_cast", "bit_expand",
                "stable_ref", "consider_as",
                "size",
                "bit_address", "bit_align"
                );

            x += PrioTable.LeftAssoc("<<","<*");

            x += PrioTable.LeftAssoc("~");
            x += PrioTable.LeftAssoc("&");
            x += PrioTable.LeftAssoc("|");

            x += PrioTable.LeftAssoc("*", "/", "\\");
            x += PrioTable.LeftAssoc("+", "-");

            x += PrioTable.LeftAssoc("<", ">", "<=", ">=");
            x += PrioTable.LeftAssoc("=", "<>");

            x += PrioTable.LeftAssoc("!~");
            x += PrioTable.LeftAssoc("!&!");
            x += PrioTable.LeftAssoc("!|!");

            x += PrioTable.RightAssoc(":=", "prototype", ":+", ":-", ":*", ":/", ":\\");

            x = x.ParLevel
                (new[]
                 {
                     "+--",
                     "+?+",
                     "?-+"
                 },
                 new[] {"then"},
                 new[] {"else"}
                );
            x += PrioTable.RightAssoc("!");
            x += PrioTable.RightAssoc(":", "function");
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
            //x.Correct("(", "<common>", '-');
            //x.Correct("[", "<common>", '-');
            //x.Correct("{", "<common>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }

        internal TokenAttribute(string token) : base(token) {}

        public TokenAttribute() : base(null) {}
    }
}