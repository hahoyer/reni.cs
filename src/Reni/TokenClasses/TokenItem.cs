using System;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class TokenItem : DumpableObject, IFormatItem
    {
        readonly bool IgnoreWhitespaces;
        readonly BinaryTree BinaryTree;

        public TokenItem(BinaryTree binaryTree, bool ignoreWhitespaces)
        {
            BinaryTree = binaryTree;
            IgnoreWhitespaces = ignoreWhitespaces;
        }

        [EnableDump]
        [Obsolete("", true)]
        SourcePart IFormatItem.Content => BinaryTree.Token;

        [EnableDump]
        bool IFormatItem.IsEssential => true;

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => BinaryTree.TokenClass;

        string IFormatItem.WhiteSpaces => IgnoreWhitespaces? "" : BinaryTree.WhiteSpaces.SourcePart.Id;
    }
}