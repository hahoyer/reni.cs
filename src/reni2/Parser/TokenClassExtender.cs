using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;

namespace Reni.Parser
{
    internal static class TokenClassExtender
    {
        [UsedImplicitly]
        internal static string Symbolize(this string token) { return token.Aggregate("", (current, tokenChar) => current + SymbolizeChar(tokenChar)); }

        private static string SymbolizeChar(Char @char)
        {
            switch(@char)
            {
                case '&':
                    return "And";
                case '\\':
                    return "Backslash";
                case ':':
                    return "Colon";
                case '.':
                    return "Dot";
                case '=':
                    return "Equal";
                case '>':
                    return "Greater";
                case '<':
                    return "Less";
                case '-':
                    return "Minus";
                case '!':
                    return "Not";
                case '|':
                    return "Or";
                case '+':
                    return "Plus";
                case '/':
                    return "Slash";
                case '*':
                    return "Star";
                case '~':
                    return "Tilde";
                case '_':
                    return "__";
                default:
                    if(Char.IsLetter(@char))
                        return "_" + @char;
                    if(Char.IsDigit(@char))
                        return @char.ToString();
                    throw new NotImplementedException("Symbolize(" + @char + ")");
            }
        }
    }
}