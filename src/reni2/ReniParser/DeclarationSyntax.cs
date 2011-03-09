using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser.TokenClasses;
using Reni.Struct;

namespace Reni.ReniParser
{
    internal sealed class DeclarationSyntax : ParsedSyntax
    {
        internal readonly DeclarationExtensionSyntax ExtensionSyntax;
        internal readonly Defineable Defineable;
        internal readonly ParsedSyntax Definition;

        internal DeclarationSyntax(Defineable defineable, TokenData token, ParsedSyntax definition)
            : this(null, defineable, token, definition) { }

        internal DeclarationSyntax(DeclarationExtensionSyntax extensionSyntax, Defineable defineable, TokenData token,
                                   ParsedSyntax definition)
            : base(token)
        {
            ExtensionSyntax = extensionSyntax;
            Defineable = defineable;
            Definition = definition;
            StopByObjectId(-876);
        }

        internal bool IsProperty { get { return ExtensionSyntax != null && ExtensionSyntax.IsProperty; } }

        internal override string DumpShort() { return Defineable.Name + ": " + Definition.DumpShort(); }

        internal override ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return Container.Create(leftToken, rightToken, this); }
    }
}