using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Formatting;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ContactType
    {
        internal static readonly ContactType AlphaNum = new ContactType();
        internal static readonly ContactType Symbol = new ContactType();
        internal static readonly ContactType Text = new ContactType();
        internal static readonly ContactType Compatible = new ContactType();

        internal bool IsCompatible(ContactType other)
        {
            if(this == Compatible)
                return true;
            if(other == Compatible)
                return true;
            return this != other;
        }

        internal static ContactType Get(ITokenClass target)
            => target == null
                ? Compatible
                : Lexer.IsAlphaLike(target.Id) || target is Number
                    ? AlphaNum
                    : target is Text
                        ? Text
                        : Lexer.IsSymbolLike(target.Id)
                            ? Symbol
                            : Compatible;
    }
}