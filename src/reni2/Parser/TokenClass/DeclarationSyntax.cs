using System;
using HWClassLibrary.Debug;
using Reni.Parser.TokenClass.Symbol;
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

        protected internal override string DumpShort()
        {
            return Name.Name + ": " + Definition.DumpShort();
        }

        protected internal override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return Container.Create(token, this);
        }
    }
}