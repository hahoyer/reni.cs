using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [Serializable]
    internal abstract class TokenClassBase : ReniObject, IIconKeyProvider
    {
        private static int _nextObjectId;
        private string _name = "";

        protected TokenClassBase()
            : base(_nextObjectId++)
        {
        }

        [Node]
        internal string Name { get { return _name; } set { _name = value; } }

        [DumpData(false)]
        internal virtual bool IsEnd { get { return false; } }

        [DumpData(false)]
        public override string NodeDump { get { return Name.Quote() + "." + ObjectId; } }

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

        [UsedImplicitly]
        internal static string Symbolize(string token)
        {
            return token.Aggregate("", (current, tokenChar) => current + SymbolizeChar(tokenChar));
        }

        private static string SymbolizeChar(Char @char)
        {
            switch (@char)
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
                        return "_"+@char;
                    if (Char.IsDigit(@char))
                        return @char.ToString();
                    throw new NotImplementedException("Symbolize(" + @char + ")");
            }
        }
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }
    }

    [Serializable]
    internal abstract class Special : TokenClassBase
    {
    }

    [Serializable]
    internal abstract class Terminal : Special, ITerminal
    {
        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new TerminalSyntax(token, this);
        }

        public abstract Result Result(ContextBase context, Category category, Token token);
    }

    [Serializable]
    internal abstract class Prefix : Special, IPrefix
    {
        public abstract Result Result(ContextBase context, Category category, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            left.AssertIsNull();
            return new PrefixSyntax(token, this, right.CheckedToCompiledSyntax());
        }
    }

    [Serializable]
    internal abstract class Infix : Special, IInfix
    {
        public abstract Result Result(ContextBase callContext, Category category, ICompileSyntax left, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return new InfixSyntax(token, left.CheckedToCompiledSyntax(), this, right.CheckedToCompiledSyntax());
        }
    }
}