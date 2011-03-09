using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base clas for compiler tokens
    /// </summary>
    [Serializable]
    internal abstract class TokenClass : Parser.TokenClass
    {
        protected override IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax((ReniParser.ParsedSyntax) left, token, (ReniParser.ParsedSyntax) right); }

        protected virtual ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal virtual ReniParser.ParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, TokenData token)
        {
            NotImplementedMethod(extensionSyntax, token);
            return null;
        }
    }
}