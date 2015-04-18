using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Brace : DumpableObject, ITreeItem
    {
        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        sealed class Factory : DumpableObject, ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left != null);

                var leftTree = (BinaryTree) left;

                Tracer.Assert(leftTree.Left == null);

                var rightParenthesis = (RightParenthesis) token.Class;
                var leftParenthesis = leftTree.Token.Class as LeftParenthesis;

                Tracer.Assert(leftParenthesis != null);
                Tracer.Assert(leftParenthesis.Level == rightParenthesis.Level);

                return new Brace
                    (leftParenthesis, leftTree.Token, leftTree.Right, token, rightParenthesis);
            }
        }

        readonly LeftParenthesis _leftClass;
        readonly TokenItem _left;
        readonly ITreeItem _target;
        readonly TokenItem _right;
        readonly RightParenthesis _rightClass;

        Brace
            (
            LeftParenthesis leftClass,
            TokenItem left,
            ITreeItem target,
            TokenItem right,
            RightParenthesis rightClass)
        {
            _leftClass = leftClass;
            _left = left;
            _target = target;
            _right = right;
            _rightClass = rightClass;
            Tracer.Assert(_rightClass.Level == _leftClass.Level);
        }

        ITokenClass ITreeItem.LeftMostTokenClass => _leftClass;
        ITokenClass ITreeItem.RightMostTokenClass => _rightClass;

        ITreeItem ITreeItem.List(List level, ListTree.Item left)
        {
            NotImplementedMethod(level, left);
            return null;
        }

        int ITreeItem.Length => _left.Length + (_target?.Length ?? 0) + _right.Length;

        int ITreeItem.UseLength(int length)
        {
            var result = length - _left.Length - _right.Length;
            if(_target != null)
                result = result <= 0 ? result : _target.UseLength(result);
            return result;
        }

        string ITreeItem.Reformat(IConfiguration configuration, ISeparatorType separator)
        {
            var inner = _target?.Reformat(configuration, separator) ?? "";
            var lines = (separator.Text + inner).Indent();
            return _left.Id + lines + separator.Text + _right.Id;
        }
    }
}