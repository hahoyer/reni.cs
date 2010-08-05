using System;
using HWClassLibrary.Debug;
using Reni.Struct;

namespace Reni.Parser.TokenClass
{
    internal sealed class DeclarationSyntax : ParsedSyntax
    {
        internal readonly DeclarationExtensionSyntax ExtensionSyntax;
        internal readonly DefineableToken Name;
        internal readonly IParsedSyntax Definition;

        internal DeclarationSyntax(DefineableToken name, Token token, IParsedSyntax definition)
            : this(null, name, token, definition)
        {
        }

        internal DeclarationSyntax(DeclarationExtensionSyntax extensionSyntax, DefineableToken name, Token token,
                                   IParsedSyntax definition)
            : base(token)
        {
            ExtensionSyntax = extensionSyntax;
            Name = name;
            Definition = definition;
            StopByObjectId(-876);
        }

        internal bool IsProperty
        {
            get { return ExtensionSyntax != null && ExtensionSyntax.IsProperty; }
        }

        protected internal override string DumpShort()
        {
            return Name.Name + ": " + Definition.DumpShort();
        }

        protected override IParsedSyntax SurroundedByParenthesis(Token leftToken, Token rightToken)
        {
            return Container.Create(leftToken,rightToken, this);
        }
    }
}