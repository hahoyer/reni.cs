using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class SmartFormat : DumpableObject
    {
        readonly IGapHandler _gapHandler;
        readonly IGapHandlerWithWhiteSpaces _gapHandlerWithWhiteSpaces;
        readonly ILineLengthLimiter _lineLengthLimiter;

        SmartFormat
            (
            IGapHandler gapHandler,
            IGapHandlerWithWhiteSpaces gapHandlerWithWhiteSpaces,
            ILineLengthLimiter lineLengthLimiter
            )
        {
            _gapHandler = gapHandler;
            _gapHandlerWithWhiteSpaces = gapHandlerWithWhiteSpaces;
            _lineLengthLimiter = lineLengthLimiter;
        }

        internal interface IGapHandler
        {
            string Gap(ITokenClass left, ITokenClass rightTokenClass);
            string StartGap(ITokenClass right);
        }

        internal interface IGapHandlerWithWhiteSpaces
        {
            string StartGap(IEnumerable<WhiteSpaceToken> rightWhiteSpaces, ITokenClass right);

            string Gap
                (ITokenClass left, IEnumerable<WhiteSpaceToken> rightWhiteSpaces, ITokenClass right);
        }

        internal interface ILineLengthLimiter
        {
            int MaxLineLength { get; }
        }

        internal static string Reformat(SourceSyntax target, SourcePart targetPart)
        {
            var smartFormat = new SmartFormat
                (
                SmartConfiguration.Instance,
                new IgnoreWhiteSpaceConfiguration(SmartConfiguration.Instance),
                new LineLengthLimiter(100)
                );
            return smartFormat.Reformat(target, 0, targetPart);
        }

        string Reformat(SourceSyntax target, int indentLevel, SourcePart targetPart)
            => Reformat(null, target, null, null, indentLevel)
                .Combine()
                .Filter(targetPart);


        IEnumerable<Item> Reformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel
            )
        {
            if(target == null)
                return Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);

            if(IsLineMode(target))
            {
                var result = LineModeReformat(target, indentLevel);
                if(result != null)
                    return result;

                if(target.IsChain())
                    return LineModeReformatOfChain(leftTokenClass, target, indentLevel);
            }

            return UncheckedReformat
                (leftTokenClass, target, rightWhiteSpaces, rightTokenClass, indentLevel);
        }

        IEnumerable<Item> UncheckedReformat
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel)
        {
            foreach(var item in Reformat
                (
                    leftTokenClass,
                    target.Left,
                    target.Token.PrecededWith,
                    target.TokenClass,
                    indentLevel
                ))
                yield return item;

            yield return new Item(target.Token, "");

            foreach(var item in Reformat
                (
                    target.TokenClass,
                    target.Right,
                    rightWhiteSpaces,
                    rightTokenClass,
                    indentLevel
                ))
                yield return item;
        }

        IEnumerable<Item> LineModeReformat(SourceSyntax target, int indentLevel)
        {
            var main = MainInformation.Create(target, this);
            if(main != null)
                return main.LineModeReformat(indentLevel);

            var brace = BraceInformation.Create(target, this);
            if(brace != null)
                return brace.LineModeReformat(indentLevel);

            return null;
        }

        internal IEnumerable<Item> LineModeReformatOfChain
            (ITokenClass leftTokenClass, SourceSyntax target, int indentLevel)
        {
            if(!target.IsChain())
            {
                foreach(var item in Reformat(leftTokenClass, target, null, null, indentLevel))
                    yield return item;
                yield break;
            }

            foreach(var item in LineModeReformatOfChain(leftTokenClass, target.Left, indentLevel))
                yield return item;


            yield return
                new Item
                    (
                    target.Token,
                    (target.Left == null ? "" : "\n" + " ".Repeat(indentLevel * 4))
                        + _gapHandlerWithWhiteSpaces.StartGap
                            (target.Token.PrecededWith, target.TokenClass));

            foreach(var item in Reformat(target.TokenClass, target.Right, null, null, indentLevel))
                yield return item;
        }

        IEnumerable<Item> IndentedReformatLineMode
            (
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int indentLevel
            )
        {
            if(target == null)
                return Gap(null, rightWhiteSpaces, rightTokenClass);

            var indent = " ".Repeat(indentLevel * 4);

            return
                ReformatLineMode(target, indentLevel)
                    .Select(item => new Item(null, indent).plus(item))
                    .PrettyLines();
        }

        IEnumerable<IEnumerable<Item>> ReformatLineMode(SourceSyntax target, int indent)
        {
            if(target.TokenClass is List)
                return ReformatListLines(target, indent);
            NotImplementedFunction(target, indent);
            return null;
        }

        IEnumerable<IEnumerable<Item>> ReformatListLines(SourceSyntax target, int indentLevel)
        {
            var separator = target.TokenClass;
            do
            {
                yield return
                    Reformat
                        (
                            null,
                            target.Left,
                            target.Token.PrecededWith,
                            target.TokenClass,
                            indentLevel
                        ).plus(new Item(target.Token, ""));
                target = target.Right;
            } while(target != null && target.TokenClass == separator);

            if(target != null)
                yield return Reformat(null, target, null, null, indentLevel);
        }

        bool IsLineMode(SourceSyntax target)
        {
            var braceInformation = BraceInformation.Create(target, this);
            if(braceInformation != null)
                return braceInformation.IsLineMode;

            return GetLineLengthInformation
                (null, target, null, null, _lineLengthLimiter.MaxLineLength) <= 0;
        }

        sealed class MainInformation : DumpableObject
        {
            readonly SmartFormat _parent;
            readonly SourceSyntax _body;
            readonly WhiteSpaceToken[] _whiteSpaces;
            readonly EndToken _end;

            MainInformation
                (SmartFormat parent, SourceSyntax body, WhiteSpaceToken[] whiteSpaces, EndToken end)
            {
                _parent = parent;
                _body = body;
                _whiteSpaces = whiteSpaces;
                _end = end;
            }

            internal static MainInformation Create(SourceSyntax target, SmartFormat parent)
            {
                var end = target.TokenClass as EndToken;
                if(end == null)
                    return null;

                Tracer.Assert(target.Right == null);
                return new MainInformation(parent, target.Left, target.Token.PrecededWith, end);
            }

            public IEnumerable<Item> LineModeReformat(int indentLevel)
                => _parent
                    .IndentedReformatLineMode(_body, _whiteSpaces, _end, indentLevel);
        }

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
                                _parent._lineLengthLimiter.MaxLineLength
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

        bool Contains(WhiteSpaceToken[] whiteSpaces, ITokenClass tokenClass)
            => _gapHandlerWithWhiteSpaces
                .StartGap(whiteSpaces, tokenClass)
                .Contains("\n");

        int GetLineLengthInformation
            (
            ITokenClass leftTokenClass,
            SourceSyntax target,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass,
            int lengthRemaining)
        {
            if(target == null)
            {
                var gap = Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);
                if(gap.Any(item => item.WhiteSpaces.Contains("\n")))
                    return 0;
                return lengthRemaining - gap.Sum(item => item.Length);
            }

            lengthRemaining = GetLineLengthInformation
                (
                    leftTokenClass,
                    target.Left,
                    target.Token.PrecededWith,
                    target.TokenClass,
                    lengthRemaining
                );
            if(lengthRemaining <= 0)
                return 0;

            lengthRemaining = lengthRemaining - target.Token.Characters.Length;
            if(lengthRemaining <= 0)
                return 0;

            lengthRemaining = GetLineLengthInformation
                (
                    target.TokenClass,
                    target.Right,
                    rightWhiteSpaces,
                    rightTokenClass,
                    lengthRemaining
                );

            return lengthRemaining;
        }

        IEnumerable<Item> Gap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass
            )
        {
            if(rightTokenClass == null)
            {
                Tracer.Assert(rightWhiteSpaces == null);
                yield break;
            }

            yield return
                new Item(null, UnfilteredGap(leftTokenClass, rightWhiteSpaces, rightTokenClass));
        }

        string UnfilteredGap
            (
            ITokenClass leftTokenClass,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass rightTokenClass)
        {
            if(leftTokenClass == null)
                return !rightWhiteSpaces.Any()
                    ? _gapHandler.StartGap(rightTokenClass)
                    : _gapHandlerWithWhiteSpaces.StartGap(rightWhiteSpaces, rightTokenClass);

            return !rightWhiteSpaces.Any()
                ? _gapHandler.Gap(leftTokenClass, rightTokenClass)
                : _gapHandlerWithWhiteSpaces.Gap(leftTokenClass, rightWhiteSpaces, rightTokenClass);
        }

        internal sealed class Item : DumpableObject
        {
            internal readonly string WhiteSpaces;
            internal readonly IToken Token;

            internal Item(IToken token, string whiteSpaces)
            {
                Token = token;
                WhiteSpaces = whiteSpaces;
                //Tracer.ConditionalBreak(Id == ";");
            }

            string Id => WhiteSpaces + (Token?.Id ?? "");
            internal int Length => Id.Length;
            protected override string GetNodeDump() => base.GetNodeDump() + " " + Id.Quote();
        }
    }


    sealed class LineLengthLimiter : SmartFormat.ILineLengthLimiter
    {
        readonly int _maxLineLength;
        public LineLengthLimiter(int maxLineLength) { _maxLineLength = maxLineLength; }
        int SmartFormat.ILineLengthLimiter.MaxLineLength => _maxLineLength;
    }
}