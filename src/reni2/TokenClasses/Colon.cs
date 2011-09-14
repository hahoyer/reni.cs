using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class Colon : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNotNull();
            return left.CreateDeclarationSyntax(token, right);
        }
    }

    [Serializable]
    internal sealed class Exclamation : TokenClass
    {
        private static readonly ITokenFactory _tokenFactory = DeclarationTokenFactory.Instance;

        [DisableDump]
        protected override ITokenFactory NewTokenFactory { get { return _tokenFactory; } }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new ExclamationSyntax(token);
        }
    }

    [Serializable]
    internal sealed class ConverterToken : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
    }

    internal abstract class DeclarationExtensionSyntax : ReniParser.ParsedSyntax
    {
        protected DeclarationExtensionSyntax(TokenData token)
            : base(token) { }

        internal virtual ReniParser.ParsedSyntax ExtendByConverter(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    internal sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        private readonly TokenData _token;

        internal ConverterDeclarationSyntax(TokenData token, TokenData otherToken)
            : base(token) { _token = otherToken; }

        internal override ReniParser.ParsedSyntax CreateDeclarationSyntax(TokenData token, ReniParser.ParsedSyntax right) { return new ConverterSyntax(_token, right.CheckedToCompiledSyntax()); }
    }

    internal sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(TokenData token)
            : base(token) { }

        internal override ReniParser.ParsedSyntax ExtendByConverter(TokenData token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}