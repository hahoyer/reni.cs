using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser.TokenClass.Symbol;
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

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }
    }

    [Serializable]
    internal abstract class Special : TokenClassBase
    {}

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
        internal abstract Result Result(ContextBase context, Category category, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            return new PrefixSyntax(token, this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }

    [Serializable]
    internal abstract class Infix : Special
    {
        internal abstract Result Result(ContextBase callContext, Category category, ICompileSyntax left, ICompileSyntax right);

        internal override sealed IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return new InfixSyntax(token, ParsedSyntax.ToCompiledSyntax(left), this, ParsedSyntax.ToCompiledSyntax(right));
        }
    }
}