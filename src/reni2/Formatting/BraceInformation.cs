using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class BraceInformation : DumpableObject
    {
        readonly SourceSyntax _body;
        readonly SmartFormat _parent;
        readonly LeftParenthesis _leftToken;
        readonly RightParenthesis _rightToken;
        readonly IToken _right;
        readonly IToken _left;

        BraceInformation
            (
            SmartFormat parent,
            IToken left,
            LeftParenthesis leftToken,
            SourceSyntax body,
            IToken right,
            RightParenthesis rightToken)
        {
            _parent = parent;
            _leftToken = leftToken;
            _body = body;
            _rightToken = rightToken;
            _right = right;
            _left = left;
            Tracer.Assert(_left != null);
            Tracer.Assert(_right != null);
        }

        internal bool IsLineMode
        {
            get
            {
                if(LeftIsLineMode && RightIsLineMode)
                    return true;
                return 0 >=
                    _parent
                        .GetLineLengthInformation
                        (
                            _leftToken,
                            _body,
                            _right.PrecededWith,
                            _rightToken,
                            _parent.LineLengthLimiter.MaxLineLength
                        );
            }
        }

        bool RightIsLineMode => _parent.Contains(_right.PrecededWith, _rightToken);
        bool LeftIsLineMode => _parent.Contains(_left.PrecededWith, _leftToken);

        internal IEnumerable<Item> LineModeReformat(int indentLevel)
        {
            var indent = " ".Repeat(indentLevel * 4);

            yield return new Item(_left, "\n" + indent);
            yield return new Item(null, "\n");

            foreach(var item in _parent.
                IndentedReformatLineMode
                (_body, _right.PrecededWith, _rightToken, indentLevel + 1))
                yield return item;

            yield return new Item(_right, "\n" + indent);
        }

        internal static BraceInformation Create(SourceSyntax target, SmartFormat parent)
        {
            var rightParenthesis = target.TokenClass as RightParenthesis;
            if(rightParenthesis == null)
                return null;

            if(target.Right != null)
                return null;

            var left = target.Left;
            Tracer.Assert(left != null, "left != null");

            var leftParenthesis = left.TokenClass as LeftParenthesis;

            if(leftParenthesis?.Level != rightParenthesis.Level || left.Left != null)
                return null;

            var rightToken = target.Token;
            var leftToken = left.Token;
            return new BraceInformation
                (
                parent,
                leftToken,
                leftParenthesis,
                left.Right,
                rightToken,
                rightParenthesis);
        }
    }
}