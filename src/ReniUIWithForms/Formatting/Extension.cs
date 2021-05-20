using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
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

        TContainer IFormatResult<int>.Concat<TContainer>(string token, TContainer other)
            => new TContainer {Value = Value + token.Length + other.Value};

        int IFormatResult<int>.Value
        {
            get => Value;
            set => Value = value;
        }
    }

    sealed class StringResult : DumpableObject, IFormatResult<string>
    {
        internal string Value;

        TContainer IFormatResult<string>.Concat<TContainer>(string token, TContainer other)
            => new TContainer {Value = Value + token + other.Value};

        string IFormatResult<string>.Value
        {
            get => Value;
            set => Value = value;
        }
    }

    static class Extension
    {
        static string FlatFormat(this IToken target, int? emptyLineLimit)
        {
            if(target.PrecededWith.Any(item => item.IsComment() && item.HasLines()))
                return null;

            if(emptyLineLimit != 0 && target.PrecededWith.Any(item => item.IsLineEnd()))
                return null;

            var result = target
                .PrecededWith
                .Where(item => item.IsComment())
                .Aggregate("", (current, item) => current + item.SourcePart.Id);

            return result + target.Characters.Id;
        }

        internal static string FlatFormat
            (this SourcePart target, IEnumerable<IItem> precede, bool areEmptyLinesPossible)
        {
            if(precede == null)
                return target.Id;

            if(precede.Any(item => item.IsComment() && item.HasLines()))
                return null;

            if(areEmptyLinesPossible && precede.Any(item => item.IsLineEnd()))
                return null;

            var result = precede
                .Where(item => item.IsComment())
                .Aggregate("", (current, item) => current + item.SourcePart.Id);

            return result + target.Id;
        }

        public static ISourcePartEdit[] GetWhiteSpaceEdits
            (this BinaryTree target, Configuration configuration, int lineBreakCount= 0)
        {
            if(target == null)
                return new ISourcePartEdit[0];
            var isSeparatorRequired = target.IsSeparatorRequired;
            var token = target.Token;
            return T(
                token.PrecededWith.Any()
                    ? new WhiteSpaceView(token.PrecededWith, configuration, isSeparatorRequired, lineBreakCount)
                    : (ISourcePartEdit)new EmptyWhiteSpaceView(token.Characters, isSeparatorRequired, lineBreakCount)
            );
        }

        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}