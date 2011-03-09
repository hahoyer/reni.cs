using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.ReniParser.TokenClasses
{
    /// <summary>
    ///     Base clas for compiler tokens
    /// </summary>
    [Serializable]
    internal abstract class TokenClass : Parser.TokenClasses.TokenClass
    {
        protected override IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax((ParsedSyntax) left, token, (ParsedSyntax) right); }

        protected virtual ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal virtual ParsedSyntax CreateDeclarationPartSyntax(DeclarationExtensionSyntax extensionSyntax, TokenData token)
        {
            NotImplementedMethod(extensionSyntax, token);
            return null;
        }
    }
}