using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace Reni.Formatting
{
    sealed class BinaryTree : DumpableObject
    {
        [EnableDump]
        readonly ITokenClass _tokenClass;

        internal readonly BinaryTree Left;
        internal WhiteSpaceToken[] TokenHead;
        internal string Token;
        internal WhiteSpaceToken[] TokenTail;
        internal readonly BinaryTree Right;

        public BinaryTree
            (
            ITokenClass tokenClass,
            BinaryTree left,
            IEnumerable<WhiteSpaceToken> tokenHead,
            string token,
            IEnumerable<WhiteSpaceToken> tokenTail,
            BinaryTree right)
        {
            _tokenClass = tokenClass;
            Left = left;
            TokenHead = tokenHead.ToArray();
            Token = token;
            TokenTail = tokenTail.ToArray();
            Right = right;
        }

        public string Reformat(IConfiguration configuration)
            => configuration.Assess(this).Reformat(this, configuration);
    }
}