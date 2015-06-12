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
    public sealed class Provider : DumpableObject
    {
        public int? MaxLineLength = 100;
        public int? EmptyLineLimit = 1;
        public int MinImprovementOfLineBreak = 10;
        public string IndentItem = "    ";

        internal string Reformat(SourceSyntax target, SourcePart targetPart)
            => Frame.CreateFrame(target, this)
                .GetItems()
                .Filter(targetPart);

        internal bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(EmptyLineLimit == null)
                return true;
            if(tokenClass is RightParenthesis)
                return false;
            if(tokenClass is LeftParenthesis)
                return false;
            if(tokenClass is List)
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
                : leftTokenClass == null ? "" : " ";

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

            return result == ""
                ? SeparatorType.Get(leftTokenClass, rightTokenClass).Text
                : result;
        }
    }
}