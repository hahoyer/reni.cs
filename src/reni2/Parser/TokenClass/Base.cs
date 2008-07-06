using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Syntax;
using System;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [AdditionalNodeInfo("NodeDump")]
    internal abstract class TokenClassBase : ReniObject
    {
        private static int _nextObjectId;

        protected TokenClassBase() : base(_nextObjectId++) {}

        [DumpData(false)]
        internal virtual bool IsEnd { get { return false; } }

        internal virtual bool IsSymbol { get { return false; } }

        internal virtual IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

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
    }

    internal abstract class Special : TokenClassBase
    {
        internal abstract string DumpShort();
    }

    internal abstract class Terminal : Special
    {
        internal abstract Result Result(ContextBase context, Category category, Token token);

        sealed internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            ParsedSyntax.IsNull(right);
            return new TerminalSyntax(token, this);
        }
    }

    internal abstract class Prefix : Special
    {
        internal abstract Result Result(ContextBase context, Category category, Token token, ICompileSyntax right);

        sealed internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            return new PrefixSyntax(token, this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }


    internal abstract class Infix : Special
    {
        internal abstract Result Result(ContextBase context, Category category, ICompileSyntax left, Token token, ICompileSyntax right);

        sealed internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return new InfixSyntax(token, ParsedSyntax.ToCompiledSyntax(left), this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }

}