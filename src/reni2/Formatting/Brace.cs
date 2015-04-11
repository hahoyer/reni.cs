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

                return new Brace(leftParenthesis.Level, leftTree.Token, leftTree.Right, token);
            }
        }

        readonly int _level;
        internal TokenItem Left;
        internal ITreeItem Target;
        internal TokenItem Right;

        Brace(int level, TokenItem left, ITreeItem target, TokenItem right)
        {
            _level = level;
            Left = left;
            Target = target;
            Right = right;
        }

        ITreeItem ITreeItem.List(List level, ListItem left)
        {
            NotImplementedMethod(level, left);
            return null;
        }

        IAssessment ITreeItem.Assess(IAssessor assessor)
        {
            var result = assessor.Brace(_level);
            if(!result.IsMaximal && Target != null)
                result = result.Combine(Target.Assess(assessor));

            return result;
        }

        string ITreeItem.Reformat(ISubConfiguration configuration)
        {
            var left = configuration.Reformat(Left);
            var target = Target?.Reformat(configuration).Indent() ?? "";
            var right = configuration.Reformat(Right);
            return left + target + right;
        }

        int ITreeItem.Length => Left.Length + (Target?.Length ?? 0) + Right.Length;
    }
}