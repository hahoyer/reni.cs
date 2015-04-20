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

                if(leftTree.Left != null)
                    return BinaryTree.FactoryInstance.Create(left, token, right);

                var rightParenthesis = (RightParenthesis) token.Class;
                var leftParenthesis = leftTree.Token.Class as LeftParenthesis;

                Tracer.Assert(leftParenthesis != null);
                Tracer.Assert(leftParenthesis.Level == rightParenthesis.Level);

                return new Brace
                    (leftParenthesis, leftTree.Token, leftTree.Right, token, rightParenthesis);
            }
        }

        [EnableDump]
        readonly LeftParenthesis _leftClass;
        [EnableDump]
        readonly TokenItem _left;
        [EnableDump]
        readonly ITreeItem _target;
        [EnableDump]
        readonly TokenItem _right;
        [EnableDump]
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

        int ITreeItem.UseLength(int length)
        {
            var result = length - _left.Length - _right.Length;
            if (_target != null)
                result = result <= 0 ? result : _target.UseLength(result);
            return result;
        }

        string ITreeItem.DefaultReformat => DefaultFormat.Instance.Reformat(this);

        ITreeItem ITreeItem.List(List level, ListTree.Item left)
        {
            NotImplementedMethod(level, left);
            return null;
        }

        string ITreeItem.Reformat(IConfiguration configuration)
        {
            NotImplementedMethod(configuration);

            var separator = UseLength(DefaultFormat.MaxLineLength) > 0
                ? SeparatorType.Contact
                : SeparatorType.Multiline;

            var inner = _target?.Reformat(configuration) ?? "";
            var lines = (separator.Text + inner).Indent();
            var result = _left.Id + lines + separator.Text + _right.Id;
            Tracer.ConditionalBreak(_leftClass.Level == 1);
            return result;
        }

        int UseLength(int length)
        {
            var result = length - _left.RightLength - _right.LeftLength;
            if (_target != null)
                result = result <= 0 ? result : _target.UseLength(result);
            return result;
        }
    }
}