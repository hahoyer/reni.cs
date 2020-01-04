using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class Extension
    {
        interface IFormatResult<TValue>
        {
            TValue Value { get; set; }

            TContainer Concat<TContainer>(string token, TContainer other)
                where TContainer : class, IFormatResult<TValue>, new();
        }

        sealed class IntegerResult : DumpableObject, IFormatResult<int>
        {
            internal int Value;
            int IFormatResult<int>.Value { get => Value; set => Value = value; }

            TContainer IFormatResult<int>.Concat<TContainer>(string token, TContainer other)
                => new TContainer { Value = Value + token.Length + other.Value };
        }

        sealed class StringResult : DumpableObject, IFormatResult<string>
        {
            internal string Value;
            string IFormatResult<string>.Value { get => Value; set => Value = value; }

            TContainer IFormatResult<string>.Concat<TContainer>(string token, TContainer other)
                => new TContainer { Value = Value + token + other.Value };
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

        static ISeparatorType LeftSideSeparator(this Syntax target)
        {
            var left = target.LeftNeighbor?.TokenClass;
            return target.Token.PrecededWith.HasComment()
                ? SeparatorType.ContactSeparator
                : SeparatorType.Get(left, target.TokenClass);
        }

        internal static ISeparatorType RightSideSeparator(this Syntax target)
        {
            var right = target.RightNeighbor;
            return right == null || right.Token.PrecededWith.HasComment()
                ? SeparatorType.ContactSeparator
                : SeparatorType.Get(target.TokenClass, right.TokenClass);
        }

        static TContainer FlatFormat<TContainer, TValue>(this Syntax target, int? emptyLineLimit)
            where TContainer : class, IFormatResult<TValue>, new()
        {

            var tokenString = target
                .Token
                .FlatFormat(emptyLineLimit);

            if(tokenString == null)
                return null;

            tokenString = target.LeftSideSeparator().Text + tokenString + target.RightSideSeparator().Text;

            var leftResult = target.Left.FlatSubFormat<TContainer, TValue>(emptyLineLimit);
            if(leftResult == null)
                return null;

            var rightResult = target.Right.FlatSubFormat<TContainer, TValue>(emptyLineLimit);
            if(rightResult == null)
                return null;

            return leftResult.Concat(tokenString, rightResult);
        }

        static TContainer FlatSubFormat<TContainer, TValue>(this Syntax left, int? emptyLineLimit)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null ? new TContainer() : left.FlatFormat<TContainer, TValue>(emptyLineLimit);

        internal static bool IsLineBreakRequired(this Syntax syntax, int? emptyLineLimit, int? maxLineLength)
        {
            var basicLineLength = syntax.FlatFormat<IntegerResult, int>(emptyLineLimit)?.Value;
            return basicLineLength == null || basicLineLength > maxLineLength;
        }

        internal static string FlatFormat(this Syntax target, int? emptyLineLimit)
            => target.FlatFormat<StringResult, string>(emptyLineLimit)?.Value;
    }
}