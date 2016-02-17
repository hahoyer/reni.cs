using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    public sealed class HierachicalFormatter : DumpableObject, IFormatter
    {
        public int? MaxLineLength = 100;
        public int? EmptyLineLimit = 1;
        public string IndentItem = "    ";

        string IFormatter.Reformat(SourceSyntax target, SourcePart targetPart)
            => Frame.Create(target, this).ItemsForResult.Filter(targetPart);

        bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(EmptyLineLimit == null)
                return true;

            if(tokenClass is RightParenthesis
                || tokenClass is LeftParenthesis
                || tokenClass is List)
                return false;

            return emptyLines < EmptyLineLimit.Value;
        }

        internal string InternalGetWhitespaces
            (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> whiteSpaces,
            ITokenClass rightTokenClass
            )
        {
            var indent = IndentItem.Repeat(indentLevel);
            var result = leadingLineBreaks > 0
                ? "\n".Repeat(leadingLineBreaks)
                : "";

            var emptyLines = leadingLineBreaks;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach
                (
                var token in
                    whiteSpaces.Where(token => !Lexer.IsWhiteSpace(token)))
            {
                if(Lexer.IsLineEnd(token)
                    && !IsRelevantLineBreak(emptyLines, rightTokenClass))
                    continue;

                if(isBeginOfLine && !Lexer.IsLineEnd(token))
                    result += indent;

                result += token.Characters.Id;

                if(Lexer.IsLineEnd(token))
                    emptyLines++;
                else
                    emptyLines = Lexer.IsLineComment(token) ? 1 : 0;

                isBeginOfLine = !Lexer.IsComment(token);
            }

            if(isBeginOfLine)
                result += indent;

            if(result != "")
                return result;

            return SeparatorType.Get(leftTokenClass, rightTokenClass).Text;
        }

        internal Item Item
            (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IToken token,
            ITokenClass tokenClass)
        {
            var whiteSpaces = InternalGetWhitespaces
                (
                    leftTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    token.PrecededWith,
                    tokenClass
                );
            return new Item(whiteSpaces, token);
        }

        internal Item Item(Frame target, int leadingLineBreaks = 0)
        {
            var leftTokenClass = target.LeftNeighbor?.Target.TokenClass;
            var indentLevel = target.IndentLevel;
            var token = target.Target.Token;
            var tokenClass = target.Target.TokenClass;
            return Item
                (
                    leftTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    token,
                    tokenClass
                );
        }

        internal bool RequiresLineBreak(string flatText)
        {
            return flatText.Any(item => item == '\n')
                || flatText.Length > MaxLineLength;
        }
    }
}