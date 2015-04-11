using System;
using System.Collections.Generic;
using hw.Parser;
using System.Linq;
using hw.Debug;
using Reni.ReniParser;

namespace Reni.Formatting
{
    sealed class TokenItem : DumpableObject
    {
        internal readonly ITokenClass Class;
        internal readonly WhiteSpaceToken[] Head;
        internal readonly string Id;
        internal readonly WhiteSpaceToken[] Tail;

        public TokenItem
            (ITokenClass @class, WhiteSpaceToken[] head, string id, WhiteSpaceToken[] tail)
        {
            Class = @class;
            Head = head;
            Id = id;
            Tail = tail;
        }

        [DisableDump]
        internal string FullText
            => (Head.SourcePart()?.Id ?? "") +
                Id +
                (Tail.SourcePart()?.Id ?? "");

        [DisableDump]
        internal int Length => Head.Length() + Id.Length + Tail.Length();

        [DisableDump]
        internal string HeadComments => Head.OnlyComments().Id();

        [DisableDump]
        internal string TailComments => Tail.OnlyComments().Id();
    }
}