using System;
using hw.DebugFormatter;
using hw.Parser;
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
        [Obsolete("",true)]
        SourcePart IFormatItem.Content => BinaryTree.Token.Characters;

        [EnableDump]
        bool IFormatItem.IsEssential => true;

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => BinaryTree.TokenClass;

        bool IFormatItem.HasEssentialWhiteSpaces
        {
            get
            {
                var tokenPrecededWith = BinaryTree.Token.PrecededWith;
                return !IgnoreWhitespaces && tokenPrecededWith.HasComment();
            }
        }

        string IFormatItem.WhiteSpaces => IgnoreWhitespaces ? "" : BinaryTree.Token.PrecededWith.SourcePart()?.Id ?? "";
    }
}