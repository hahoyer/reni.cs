using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [AdditionalNodeInfo("NodeDump")]
    internal abstract class Base : ReniObject
    {
        private static int _nextObjectId;

        protected Base() : base(_nextObjectId++) {}

        /// <summary>
        /// true only for end token
        /// </summary>
        /// <returns></returns>
        [DumpData(false)]
        internal virtual bool IsEnd { get { return false; } }

        internal virtual bool IsSymbol { get { return false; } }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal virtual Syntax.SyntaxBase CreateSyntax(Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        /// <summary>
        /// The name of the token for lookup in prio table of parser.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// created 31.03.2007 23:36 on SAPHIRE by HH
        internal virtual string PrioTableName(string name)
        {
            return name;
        }

        internal static bool IsTokenType(string typeName, bool isSymbol, string token)
        {
            return typeName.EndsWith(TokenToTypeNameEnd(isSymbol, token));
        }

        internal static string TokenToTypeNameEnd(bool isSymbol, string token)
        {
            if(isSymbol)
                return ".TokenClass.Symbol." + Symbolize(token);
            return ".TokenClass.Name.T" + token + "T";
        }

        /// <summary>
        /// Symbolizes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:49]
        internal static string Symbolize(string token)
        {
            var name = "";
            for(var i = 0; i < token.Length; i++)
                name += SymbolizeChar(token[i]);
            return name;
        }

        /// <summary>
        /// Symbolizes the char.
        /// </summary>
        /// <param name="Char">The char.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:38]
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
    }

    internal abstract class Special: Base
    {
        protected Syntax.SyntaxBase CreateSpecialSyntax(Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
        {
            return new Syntax.Special(left, token, this, right);
        }

        /// <summary>
        /// Results the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        internal virtual Result Result(Context.ContextBase context, Category category, Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
        {
            NotImplementedMethod(context, category, left, token, right);
            return null;
        }
    }
}