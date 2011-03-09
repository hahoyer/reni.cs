using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Parser.TokenClasses;

namespace Reni.ReniParser.TokenClasses
{
    /// <summary>
    ///     Tokens that can used in definitions (not reserved tokens)
    /// </summary>
    [Serializable]
    internal abstract class Defineable : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(this, token);
            if(left == null)
                return new ExpressionSyntax(this, null, token, right.ToCompiledSyntaxOrNull());
            return left.CreateSyntaxOrDeclaration(this, token, right);
        }

        internal override ParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, TokenData token)
        {
            return new DeclarationPartSyntax(this, extensionSyntax, token);
        }

        internal TFeatureType Check<TFeatureType>()
            where TFeatureType : class { return this as TFeatureType; }

        protected string DataFunctionName { get { return Name.Symbolize(); } }
    }

    internal sealed class DeclarationPartSyntax : ParsedSyntax
    {
        private readonly Defineable _defineable;
        private readonly DeclarationExtensionSyntax _extensionSyntax;

        internal DeclarationPartSyntax(Defineable defineable, DeclarationExtensionSyntax extensionSyntax, TokenData token)
            : base(token)
        {
            _defineable = defineable;
            _extensionSyntax = extensionSyntax;
        }

        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right) { return new DeclarationSyntax(_extensionSyntax, _defineable, token, right); }
    }
}