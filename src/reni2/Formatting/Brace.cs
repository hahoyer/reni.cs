using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;

namespace Reni.Formatting
{
    sealed class Brace : DumpableObject, ITreeItem
    {
        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        sealed class Factory : DumpableObject, ITreeItemFactory
        {}

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

        IAssessment ITreeItem.Assess(IAssessor assessor)
        {
            var result = assessor.Brace(_level);
            if(!result.IsMaximal && Target != null)
                result = result.plus(Target.Assess(assessor));

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