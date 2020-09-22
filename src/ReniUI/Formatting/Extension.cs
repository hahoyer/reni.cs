using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class Extension
    {
        interface IFormatResult<TValue>
        {
            TValue Value {get; set;}

            TContainer Concat<TContainer>(string token, TContainer other)
                where TContainer : class, IFormatResult<TValue>, new();
        }

        sealed class IntegerResult : DumpableObject, IFormatResult<int>
        {
            internal int Value;
            int IFormatResult<int>.Value {get => Value; set => Value = value;}

            TContainer IFormatResult<int>.Concat<TContainer>(string token, TContainer other)
                => new TContainer {Value = Value + token.Length + other.Value};
        }

        sealed class StringResult : DumpableObject, IFormatResult<string>
        {
            internal string Value;
            string IFormatResult<string>.Value {get => Value; set => Value = value;}

            TContainer IFormatResult<string>.Concat<TContainer>(string token, TContainer other)
                => new TContainer {Value = Value + token + other.Value};
        }

        static string FlatFormat(this IToken target, int? emptyLineLimit)
        {
            if(target.PrecededWith.Any(item => item.IsComment() && item.HasLines()))
                return null;

            if(emptyLineLimit != 0 && target.PrecededWith.Any(item => item.IsLineBreak()))
                return null;

            var result = target
                .PrecededWith
                .Where(item => item.IsComment())
                .Aggregate(seed: "", func: (current, item) => current + item.SourcePart.Id);

            return result + target.Characters.Id;
        }

        static string FlatFormat(this SourcePart target, IEnumerable<IItem> precede, bool areEmptyLinesPossible)
        {
            if(precede.Any(item => item.IsComment() && item.HasLines()))
                return null;

            if(areEmptyLinesPossible && precede.Any(item => item.IsLineBreak()))
                return null;

            var result = precede
                .Where(item => item.IsComment())
                .Aggregate(seed: "", func: (current, item) => current + item.SourcePart.Id);

            return result + target.Id;
        }

        static TContainer FlatFormat<TContainer, TValue>(this Helper.Syntax target, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var left = target.Left?.FlatFormat<TContainer, TValue>(areEmptyLinesPossible);
            var main = target.MainToken.Id;
            var right = target.Right?.FlatFormat<TContainer, TValue>(areEmptyLinesPossible);

            if(target.TokenClass is BeginOfText)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right != null);

                Tracer.ConditionalBreak(target.RightWhiteSpaces.Any());
            }

            else if(target.TokenClass is List || target.TokenClass is Colon)
            {
                Tracer.ConditionalBreak(target.Right == null);
                Tracer.ConditionalBreak(target.Left == null);

                Tracer.ConditionalBreak(target.LeftWhiteSpaces.Any());
                Tracer.ConditionalBreak(target.RightWhiteSpaces.Any());
                main = main + " ";
            }

            else if (target.TokenClass is Definable)
            {
                Tracer.Assert(target.Left == null);
                Tracer.Assert(target.Right == null);
            }

            else if (target.TokenClass is EndOfText)
            {
                Tracer.Assert(target.Left != null);
                Tracer.Assert(target.Right == null);

                Tracer.ConditionalBreak(target.LeftWhiteSpaces.Any());
            }

            else
            {
                Tracer.DumpStaticMethodWithData(target, areEmptyLinesPossible);


                var tokenString = target
                    .MainToken
                    .FlatFormat(target.LeftWhiteSpaces, areEmptyLinesPossible);

                if(tokenString == null)
                    return null;

                tokenString = (target.LeftSideSeparator?" ":"") + tokenString;

                var leftResult = target.Left.FlatSubFormat<TContainer, TValue>(areEmptyLinesPossible);
                if(leftResult == null)
                    return null;

                var rightResult = target.Right.FlatSubFormat<TContainer, TValue>(areEmptyLinesPossible);
                return rightResult == null ? null : leftResult.Concat(tokenString, rightResult);
            }

            return (left ?? new TContainer()).Concat(main, right ?? new TContainer());
        }

        static TContainer FlatSubFormat<TContainer, TValue>(this Helper.Syntax left, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null ? new TContainer() : left.FlatFormat<TContainer, TValue>(areEmptyLinesPossible);

        internal static bool HasAlreadyLineBreakOrIsTooLong(this Helper.Syntax syntax, int? maxLineLength, bool areEmptyLinesPossible)
        {
            var basicLineLength = syntax.GetFlatLength(areEmptyLinesPossible);
            return basicLineLength == null || basicLineLength > maxLineLength;
        }

        /// <summary>
        /// Try to format target into one line.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The formatted line or null if target contains line breaks.</returns>
        internal static string FlatFormat(this Helper.Syntax target, bool areEmptyLinesPossible)
            => target.FlatFormat<StringResult, string>(areEmptyLinesPossible)?.Value;

        /// <summary>
        /// Get the line length of target when formatted as one line.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The line length calculated or null if target contains line breaks.</returns>
        internal static int? GetFlatLength(this Helper.Syntax target, bool areEmptyLinesPossible)
            => target.FlatFormat<IntegerResult, int>(areEmptyLinesPossible)?.Value;
    }
}