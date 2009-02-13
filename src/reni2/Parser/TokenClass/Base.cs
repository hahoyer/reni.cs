using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser.TokenClass.Symbol;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    internal sealed class TokenAttribute : TokenAttributeBase
    {
        internal override PrioTable CreatePrioTable()
        {
            var x = PrioTable.LeftAssoc("<else>");
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
            //x.Correct("(", "<else>", '-');
            //x.Correct("[", "<else>", '-');
            //x.Correct("{", "<else>", '-');

            //Tracer.FlaggedLine("\n"+x.ToString());
            return x;
        }

        internal TokenAttribute(string token) : base(token) {}

        public TokenAttribute() : base(null) {}
    }

    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [Serializable]
    internal abstract class TokenClassBase : ReniObject, IIconKeyProvider
    {
        private static int _nextObjectId;
        private string _tokenCache;

        protected TokenClassBase() : base(_nextObjectId++) {}

        [Node]
        internal virtual string Name
        {
            get
            {
                if(_tokenCache == null)
                {
                    _tokenCache = "";
                    var attributes = GetType().GetCustomAttributes(typeof(TokenAttribute), true);
                    if(attributes.Length == 1)
                        _tokenCache = ((TokenAttribute) (attributes[0])).Token;

                }
                return _tokenCache;
            }
        }
        [DumpData(false)]
        internal virtual bool IsEnd { get { return false; } }

        [DumpData(false)]
        public override string NodeDump { get { return Name.Quote()+"."+ObjectId; } }
        [DumpData(false)]
        internal virtual TokenFactory NewTokenFactory { get { return null; } }

        internal virtual IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal virtual IParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, Token token)
        {
            NotImplementedMethod(extensionSyntax, token);
            return null;
        }

        internal virtual string PrioTableName(string name)
        {
            return name;
        }

        internal static string Symbolize(string token)
        {
            var name = "";
            for(var i = 0; i < token.Length; i++)
                name += SymbolizeChar(token[i]);
            return name;
        }

        internal static string SymbolizeChar(char Char)
        {
            switch(Char)
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
                default:
                    throw new NotImplementedException("Symbolize(" + Char + ")");
            }
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }
    }

    [Serializable]
    internal abstract class Special : TokenClassBase
    {
        internal string DumpShort()
        {
            return Name;
        }
    }

    [Serializable]
    internal abstract class Terminal : Special
    {
        internal abstract Result Result(ContextBase context, Category category, Token token);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            ParsedSyntax.IsNull(right);
            return new TerminalSyntax(token, this);
        }
    }

    [Serializable]
    internal abstract class Prefix : Special
    {
        internal abstract Result Result(ContextBase context, Category category, Token token, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            return new PrefixSyntax(token, this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }

    [Serializable]
    internal abstract class Infix : Special
    {
        internal abstract Result Result(ContextBase callContext, Category category, ICompileSyntax left, Token token, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return new InfixSyntax(token, ParsedSyntax.ToCompiledSyntax(left), this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }
}