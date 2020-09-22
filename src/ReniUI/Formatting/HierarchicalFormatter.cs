using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class HierarchicalFormatter : DumpableObject, IFormatter
    {
        static bool IsRelevant(ITokenClass tokenClass)
            => (tokenClass as TokenClass)?.IsVisible ?? true;

        readonly Configuration Configuration;

        public HierarchicalFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compilerBrowser, SourcePart targetPart)
        {
            var result = Frame.Create
                (this, compilerBrowser, targetPart)
                .ItemsForResult;

            return result.GetEditPieces(targetPart);
        }

        bool IsRelevantLineBreak(int emptyLines, ITokenClass tokenClass)
        {
            if(Configuration.EmptyLineLimit == null)
                return true;

            if(tokenClass is RightParenthesis || tokenClass is LeftParenthesis || tokenClass is List)
                return false;

            return emptyLines < Configuration.EmptyLineLimit.Value;
        }

        ResultItems InternalGetWhitespaces
        (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IEnumerable<IItem> whiteSpaces,
            ITokenClass rightTokenClass
        )
        {
            var result = ResultItems.Default();
            var emptyLines = 0;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach(var token in whiteSpaces)
            {
                if(isBeginOfLine && !Lexer.IsLineEnd(token))
                {
                    result.AddLineBreak(leadingLineBreaks - emptyLines);
                    result.AddSpaces(indentLevel * Configuration.IndentCount);
                    emptyLines = leadingLineBreaks;
                    leadingLineBreaks = 0;
                    isBeginOfLine = false;
                }

                if(Lexer.IsSpace(token) ||
                   Lexer.IsLineEnd(token) && !IsRelevantLineBreak(emptyLines, rightTokenClass))
                    result.AddHidden(token);
                else
                {
                    result.Add(token);

                    if(Lexer.IsLineEnd(token))
                        emptyLines++;
                    else
                        emptyLines = Lexer.IsLineComment(token) ? 1 : 0;

                    isBeginOfLine = !Lexer.IsMultiLineComment(token);
                }
            }

            if(isBeginOfLine)
            {
                result.AddLineBreak(leadingLineBreaks - emptyLines);
                result.AddSpaces(indentLevel * Configuration.IndentCount);
                leadingLineBreaks = 0;
            }

            if(result.IsEmpty && SeparatorExtension.Get(leftTokenClass, rightTokenClass))
                result.AddSpaces(1);

            Tracer.Assert(leadingLineBreaks == 0);

            return result;
        }

        ResultItems Item
        (
            ITokenClass leftTokenClass,
            int leadingLineBreaks,
            int indentLevel,
            IEnumerable<IItem> precede,
            SourcePart main,
            ITokenClass tokenClass)
        {
            Tracer.Assert(IsRelevant(tokenClass));
            Tracer.Assert(IsRelevant(leftTokenClass));

            var trace = tokenClass.Id == " repeat";
            StartMethodDump
                (trace, leftTokenClass, leadingLineBreaks, indentLevel, precede, main, tokenClass);
            try
            {
                BreakExecution();
                var result = InternalGetWhitespaces
                (
                    leftTokenClass,
                    leadingLineBreaks,
                    indentLevel,
                    precede,
                    tokenClass
                );

                result.Add(main);

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
                return ResultItems.Default();

            var leftNeighbor = target
                .LeftNeighbor?
                .Chain(item => item.LeftNeighbor)
                .FirstOrDefault(item => IsRelevant(item.Target.TokenClass));

            var leftTokenClass = leftNeighbor?.Target.TokenClass;
            var indentLevel = target.IndentLevel;
            var leftWhiteSpaces = target.Target.Target.Token.PrecededWith;
            var main = target.Target.Target.Token.Characters;
            return Item
            (
                leftTokenClass,
                leadingLineBreaks,
                indentLevel,
                leftWhiteSpaces,
                main,
                tokenClass
            );
        }

        internal bool RequiresLineBreak(string flatText)
        {
            return flatText.Any(item => item == '\n') || flatText.Length > Configuration.MaxLineLength;
        }
    }
}