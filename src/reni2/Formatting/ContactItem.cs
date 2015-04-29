using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class ContactItem : DumpableObject
    {
        [EnableDump]
        readonly ITokenClass _leftTokenClass;
        readonly SourceSyntax _right;

        internal ContactItem(ITokenClass leftTokenClass, SourceSyntax right)
        {
            _leftTokenClass = leftTokenClass;
            _right = right;
        }

        [DisableDump]
        internal ISeparatorType SeparatorType
        {
            get
            {
                if(_leftTokenClass == null)
                    return Formatting.SeparatorType.None;
                if(RightTokenClass is List)
                    return Formatting.SeparatorType.Contact;

                return null;
            }
        }

        [EnableDump]
        ITokenClass RightTokenClass => _right.TokenClass;

        internal string Format(SourcePart targetPart)
        {
            NotImplementedMethod(targetPart);
            return null;
        }
    }
}