using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Feature;
using Reni.Parser.TokenClass.Symbol;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    [Serializable]
    internal abstract class Defineable : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(token);
            if(left == null)
                return new ExpressionSyntax(null, token, ParsedSyntaxExtender.ToCompiledSyntaxOrNull(right));
            return left.CreateSyntaxOrDeclaration(token, right);
        }

        internal override IParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax,
                                                                    Token token)
        {
            return new DeclarationPartSyntax(extensionSyntax, token);
        }

        internal TFeatureType Check<TFeatureType>() 
            where TFeatureType : class
        {
            return this as TFeatureType;
        }

        protected string DataFunctionName { get { return Symbolize(Name); } }
    }

    internal sealed class DeclarationPartSyntax : ParsedSyntax
    {
        private readonly DefineableToken _defineableToken;
        private readonly DeclarationExtensionSyntax _extensionSyntax;

        internal DeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, Token token)
            : base(token)
        {
            _defineableToken = new DefineableToken(token);
            _extensionSyntax = extensionSyntax;
        }

        protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_extensionSyntax, _defineableToken, token, right);
        }
    }
}