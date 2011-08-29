using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    internal abstract class Defineable : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(this, token);
            if(left == null)
                return new ExpressionSyntax(this, null, token, right.ToCompiledSyntaxOrNull());
            return left.CreateSyntaxOrDeclaration(this, token, right);
        }

        internal ReniParser.ParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, TokenData token) { return new DeclarationPartSyntax(this, extensionSyntax, token); }

        internal TFeatureType Check<TFeatureType>()
            where TFeatureType : class { return this as TFeatureType; }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }
    }

    internal sealed class DeclarationPartSyntax : ReniParser.ParsedSyntax
    {
        private readonly Defineable _defineable;
        private readonly DeclarationExtensionSyntax _extensionSyntax;

        internal DeclarationPartSyntax(Defineable defineable, DeclarationExtensionSyntax extensionSyntax, TokenData token)
            : base(token)
        {
            _defineable = defineable;
            _extensionSyntax = extensionSyntax;
        }

        internal override ReniParser.ParsedSyntax CreateDeclarationSyntax(TokenData token, ReniParser.ParsedSyntax right) { return new DeclarationSyntax(_extensionSyntax, _defineable, token, right); }
    }
}