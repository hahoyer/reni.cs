using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    sealed class ContactType
    {
        internal static readonly ContactType AlphaNum = new ContactType();
        internal static readonly ContactType Symbol = new ContactType();
        internal static readonly ContactType Text = new ContactType();
        internal static readonly ContactType Compatible = new ContactType();

        public bool IsCompatible(ContactType other)
        {
            if(this == Compatible)
                return true;
            if(other == Compatible)
                return true;
            return this != other;
        }
    }
}