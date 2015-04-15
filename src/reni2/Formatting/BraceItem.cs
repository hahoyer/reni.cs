using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class BraceItem : DumpableObject
    {
        readonly int _level;
        readonly IToken _left;
        readonly SourceSyntax _target;
        readonly IToken _right;

        BraceItem(int level, IToken left, SourceSyntax target, IToken right)
        {
            _level = level;
            _left = left;
            _target = target;
            _right = right;
        }

        internal string Reformat(DefaultFormat formatter)
        {
            var lefttoken = formatter.Format(_left);
            var rightToken = formatter.Format(_right);
            var innerTarget = _target?.Reformat(formatter);

            var separator = DefaultFormat.BraceSeparator(_level)
                .Escalate(() => DefaultFormat.AssessSeparator(innerTarget));

            return lefttoken +
                (separator.Text + innerTarget).Indent() +
                separator.Text +
                rightToken;
        }

        internal static BraceItem CheckedCreate(SourceSyntax target)
        {
            var rightParenthesis = target.TokenClass as RightParenthesis;
            if(rightParenthesis == null)
                return null;

            var left = target.Left;
            Tracer.Assert(left != null);

            var leftParenthesis = left.TokenClass as LeftParenthesis;
            if(leftParenthesis?.Level != rightParenthesis.Level)
                return null;

            Tracer.Assert(target.Right == null);
            Tracer.Assert(left.Left == null);

            return new BraceItem(rightParenthesis.Level, left.Token, left.Right, target.Token);
        }
    }
}