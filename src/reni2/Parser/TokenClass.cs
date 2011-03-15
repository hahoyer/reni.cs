using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;

namespace Reni.Parser
{
    internal abstract class TokenClass : ReniObject, IIconKeyProvider, ITokenClass
    {
        private static int _nextObjectId;
        private string _name;

        protected TokenClass()
            : base(_nextObjectId++) { }

        string ITokenClass.Name { set { _name = value; } }
        string ITokenClass.PrioTableName(string name) { return PrioTableName(name); }
        ITokenFactory ITokenClass.NewTokenFactory { get { return NewTokenFactory; } }
        IParsedSyntax ITokenClass.Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax(left, token, right); }
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }

        protected virtual IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual string PrioTableName(string name) { return Name; }
        protected virtual ITokenFactory NewTokenFactory { get { return null; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return Name.Quote() + "." + ObjectId; } }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }

        [Node, IsDumpEnabled(false)]
        
        internal string Name { get { return _name; } set { _name = value; } }
    }

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