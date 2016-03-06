using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Formatting;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public sealed class HierachicalFormatter : DumpableObject, IFormatter
    {
        readonly Configuration Configuration;
        public string IndentItem = "    ";

        public HierachicalFormatter(Configuration configuration) { Configuration = configuration; }

        string IFormatter.Reformat(SourceSyntax target, SourcePart targetPart)
            => Frame.Create(target, this).ItemsForResult.Format(targetPart);

        bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(Configuration.EmptyLineLimit == null)
                return true;

            if(tokenClass is RightParenthesis
                || tokenClass is LeftParenthesis
                || tokenClass is List)
                return false;

            return emptyLines < Configuration.EmptyLineLimit.Value;
        }

        internal ResultItems InternalGetWhitespaces
            (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> whiteSpaces,
            ITokenClass rightTokenClass
            )
        {
            var indent = IndentItem.Repeat(indentLevel);
            var result = new ResultItems();
            if(leadingLineBreaks > 0)
                result.Add("\n".Repeat(leadingLineBreaks));

            var emptyLines = leadingLineBreaks;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach(var token in whiteSpaces)
            {
                if(isBeginOfLine && !Lexer.IsLineEnd(token))
                {
                    result.Add(indent);
                    isBeginOfLine = false;
                }

                if(Lexer.IsWhiteSpace(token)
                    || (Lexer.IsLineEnd(token) && !IsRelevantLineBreak(emptyLines, rightTokenClass)))
                    result.AddHidden(token.Characters);
                else
                {
                    result.Add(token.Characters);

                    if(Lexer.IsLineEnd(token))
                        emptyLines++;
                    else
                        emptyLines = Lexer.IsLineComment(token) ? 1 : 0;

                    isBeginOfLine = !Lexer.IsComment(token);
                }
            }

            if(isBeginOfLine)
                result.Add(indent);

            if(result.IsEmpty)
                result.Add(SeparatorType.Get(leftTokenClass, rightTokenClass).Text);

            return result;
        }

        internal ResultItems Item
            (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IToken token,
            ITokenClass tokenClass)
        {
            Tracer.Assert(IsRelevant(tokenClass));
            Tracer.Assert(IsRelevant(leftTokenClass));

            var trace = tokenClass.Id == " ;";
            StartMethodDump
                (trace, leftTokenClass, leadingLineBreaks, indentLevel, token, tokenClass);
            try
            {
                BreakExecution();
                var result = InternalGetWhitespaces
                    (
                        leftTokenClass,
                        leadingLineBreaks,
                        indentLevel,
                        token.PrecededWith,
                        tokenClass
                    );

                result.Add(token.Characters);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal ResultItems Item(Frame target, int leadingLineBreaks = 0)
        {
            var tokenClass = target.Target.TokenClass;
            if(!IsRelevant(tokenClass))
                return new ResultItems();

            var leftNeighbor = target
                .LeftNeighbor?
                .Chain(item => item.LeftNeighbor)
                .FirstOrDefault(item => IsRelevant(item.Target.TokenClass));

            var leftTokenClass = leftNeighbor?.Target.TokenClass;
            var indentLevel = target.IndentLevel;
            var token = target.Target.Token;
            return Item
                (
                    leftTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    token,
                    tokenClass
                );
        }

        static bool IsRelevant(ITokenClass tokenClass)
            => (tokenClass as TokenClass)?.IsVisible ?? true;

        internal bool RequiresLineBreak(string flatText)
        {
            return flatText.Any(item => item == '\n')
                || flatText.Length > Configuration.MaxLineLength;
        }
    }
}