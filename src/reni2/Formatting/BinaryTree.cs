using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class BinaryTree : DumpableObject, ITreeItem
    {
        internal readonly ITreeItem Left;
        internal readonly TokenItem Token;
        internal readonly ITreeItem Right;

        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        sealed class Factory : ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
                => new BinaryTree(left, token, right);
        }

        BinaryTree(ITreeItem left, TokenItem token, ITreeItem right)
        {
            Left = left;
            Token = token;
            Right = right;
            Tracer.Assert(Token != null);
        }

        ITreeItem ITreeItem.List(List level, ListTree.Item left)
            => new ListTree(level, new[] {left, new ListTree.Item(this, null)});

        int ITreeItem.UseLength(int length)
        {
            var result = length - Token.Length;
            if(Left != null)
                result = result <= 0 ? result : Left.UseLength(result);
            if(Right != null)
                result = result <= 0 ? result : Right.UseLength(result);
            return result;
        }

        ITokenClass ITreeItem.LeftMostTokenClass
            => Left == null ? Token.Class : Left.LeftMostTokenClass;

        ITokenClass ITreeItem.RightMostTokenClass
            => Right == null ? Token.Class : Right.RightMostTokenClass;

        string ITreeItem.Reformat(IConfiguration configuration, ISeparatorType separator)
            => configuration.Reformat(this, separator);

        internal ISeparatorType LeftInnerSeparator()
        {
            if(Left == null)
                return null;

            var other = Left?.RightMostTokenClass;

            if(other is RightParenthesis &&
                Left.UseLength(DefaultFormat.MaxLineLength) < 0)
                return SeparatorType.Multiline;

            return DefaultFormat.Separator(other, Token.Class);
        }

        internal ISeparatorType RightInnerSeparator()
        {
            if(Right == null)
                return null;

            var other = Right.LeftMostTokenClass;

            if(other is LeftParenthesis &&
                Right.UseLength(DefaultFormat.MaxLineLength) < 0)
                return SeparatorType.Multiline;

            return DefaultFormat.Separator(Token.Class, other);
        }

        string ITreeItem.DefaultReformat => DefaultFormat.Instance.Reformat(this);
    }
}