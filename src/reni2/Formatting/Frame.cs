using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Frame : DumpableObject
    {
        static int _nextId;

        Frame(Frame parent, SourceSyntax target)
        {
            _parent = parent;
            _formatter = _parent._formatter;
            Target = target;
        }

        internal Frame(SmartFormat formatter, SourceSyntax target)
        {
            _formatter = formatter;
            Target = target;
        }

        readonly SmartFormat _formatter;
        readonly Frame _parent;
        internal readonly SourceSyntax Target;

        [DisableDump]
        Frame Left => new Frame(this, Target.Left);

        [DisableDump]
        Frame Right => new Frame(this, Target.Right);

        string Whitespaces => GetWhitespaces(LeadingLineBreaks, IndentLevel);

        string GetWhitespaces(int leadingLineBreaks, int indentLevel)
        {
            int traceId = 29;



            var indent = " ".Repeat(indentLevel * 4);
            var result = "\n".Repeat(leadingLineBreaks);
            var emptyLines = leadingLineBreaks;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach
                (
                var token in
                    Target
                        .Token
                        .PrecededWith
                        .Where(item => RelevantForWhitespaces(emptyLines, item))
                )
            {
                if(isBeginOfLine && !Lexer.IsLineEnd(token))
                {
                    Tracer.ConditionalBreak(_nextId == traceId);
                    result += (_nextId++) + indent;
                }

                result += token.Characters.Id;

                if(Lexer.IsLineEnd(token))
                    emptyLines++;
                else
                    emptyLines = Lexer.IsLineComment(token) ? 1 : 0;

                isBeginOfLine = !Lexer.IsComment(token);
            }

            if(isBeginOfLine)
            {
                Tracer.ConditionalBreak(_nextId == traceId);
                result += (_nextId++) + indent;
            }

            return result == ""
                ? SeparatorType.Get(LeftTokenClass, Target.TokenClass).Text
                : result;
        }

        bool RelevantForWhitespaces(int emptyLines, WhiteSpaceToken token)
        {
            if(Lexer.IsWhiteSpace(token))
                return false;
            if(!Lexer.IsLineEnd(token))
                return true;
            return _formatter.IsRelevantLineBreak(emptyLines, Target.TokenClass);
        }

        ITokenClass LeftTokenClass
            => Target?.Left?.RightMostTokenClass
                ?? _parent?.LeftTokenClassForChild(Target);

        ITokenClass LeftTokenClassForChild(SourceSyntax child)
            => child == Target.Right ? Target.TokenClass : _parent?.LeftTokenClassForChild(Target);

        SmartFormat.IMode Mode
        {
            get
            {
                if(Target.TokenClass is RightParenthesis)
                    if(GetWhitespaces(0, 0).Contains("\n"))
                        return LineModeRightBrace.Instance;
                return null;
            }
        }

        IEnumerable<Frame> ParentChain
        {
            get
            {
                if(_parent != null)
                {
                    yield return _parent;
                    foreach(var frame in _parent.ParentChain)
                        yield return frame;
                }
            }
        }

        int IndentLevel => ParentChain.Count(item => item.HasIndent) + (HasIndent?1:0);

        bool HasIndent
        {
            get
            {
                if(Target.TokenClass is List)
                    return false;

                return RootListFrameOfItem?.IsBraced ?? false;
            }
        }

        bool IsListItem
        {
            get
            {
                if(Target.TokenClass is List)
                    return false;

                return _parent?.Target.TokenClass is List;
            }
        }

        Frame RootListFrameOfItem => Target.TokenClass is List ? null : _parent?.RootListFrame;

        Frame RootListFrame
        {
            get
            {
                if(!(Target.TokenClass is List))
                    return null;

                if(_parent?.Target.TokenClass is List)
                    return _parent.RootListFrame;

                return this;
            }
        }

        bool IsBraced
            => _parent != null
                && Target == _parent.Target.Right
                && _parent.IsBraceConstruct;

        bool IsBraceConstruct
        {
            get
            {
                var rightBraceItem = _parent?.Target;

                var rightBrace = rightBraceItem?.TokenClass as RightParenthesis;
                if(rightBrace == null)
                    return false;

                var leftBraceItem = Target;
                if(leftBraceItem != rightBraceItem.Left)
                    return false;

                var leftBrace = leftBraceItem.TokenClass as LeftParenthesis;

                return leftBrace?.Level == rightBrace.Level;
            }
        }

        static int LeadingLineBreaks { get { return 0; } }

        [DisableDump]
        internal IEnumerable<Item> Items
        {
            get
            {
                if(Target == null)
                    return new Item[0];

                var result = new List<Item>();
                var left = Left.Items;
                result.AddRange(left);
                result.Add(new Item(Whitespaces, Target.Token));
                var right = Right.Items;
                result.AddRange(right);
                return result.ToArray();
            }
        }

        [DisableDump]
        internal string Reformat => Items.Combine().Filter();
    }

    sealed class LineModeRightBrace : SmartFormat.IMode
    {
        LineModeRightBrace() { }
        internal static readonly SmartFormat.IMode Instance = new LineModeRightBrace();
    }
}