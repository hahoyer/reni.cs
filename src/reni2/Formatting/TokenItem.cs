using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.Formatting
{
    sealed class TokenItem : DumpableObject
    {
        internal readonly ITokenClass Class;
        readonly WhiteSpaceToken[] _head;
        readonly string _id;
        readonly WhiteSpaceToken[] _tail;

        public TokenItem
            (ITokenClass @class, WhiteSpaceToken[] head, string id, WhiteSpaceToken[] tail)
        {
            Class = @class;
            _head = head;
            _id = id;
            _tail = tail;
            Tracer.Assert(_head != null);
            Tracer.Assert(_tail != null);
        }

        [DisableDump]
        internal int Length => Id.Length;
        [DisableDump]
        internal string Id => Head + _id + Tail;
        [DisableDump]
        internal int LeftLength => Head.Length + _id.Length;
        [DisableDump]
        internal int RightLength => _id.Length + Tail.Length;

        string Head => _head.OnlyComments().Id();
        string Tail => _tail.OnlyComments().Id();
    }
}