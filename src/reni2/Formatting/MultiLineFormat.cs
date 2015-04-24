using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class MultiLineFormat : DumpableObject, ISubConfiguration
    {
        readonly DefaultFormat _parent;

        internal MultiLineFormat(DefaultFormat parent) { _parent = parent; }

        string ISubConfiguration.Reformat(TokenItem target)
        {
            var head = LeadingLineBreak(target.Class);
            var tokenHead = target.HeadComments;
            var tokenTail = target.TailComments;
            if(tokenHead != "" || tokenTail != "")
                NotImplementedMethod(target);

            return (head ? "\n" : "") + target.Id;
        }

        IConfiguration ISubConfiguration.Parent => _parent;

        static bool LeadingLineBreak(ITokenClass tokenClass)
            => tokenClass is LeftParenthesis || tokenClass is RightParenthesis;

        string ISubConfiguration.ListItemHead => "\n";
    }
}