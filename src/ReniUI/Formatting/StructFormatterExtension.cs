using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
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

        internal static IStructure CreateStruct(this Syntax syntax, StructFormatter parent)
        {
            switch(syntax.TokenClass)
            {
                case RightParenthesis _: return new ParenthesisStructure(syntax, parent);
                case Number _:
                    if(syntax.Right == null || syntax.Right.TokenClass is RightParenthesis)
                        return new ChainStructure(syntax, parent);

                    Dumpable.NotImplementedFunction(syntax, parent);
                    return null;
            }

            Dumpable.NotImplementedFunction(syntax, parent);
            return null;
        }

        internal static bool LineBreakScan(this FormatterToken[] target, ref int? lineLength)
        {
            foreach(var token in target)
                if(token.LineBreakScan(ref lineLength))
                    return true;
            return false;
        }

        internal static IStructure CreateBodyStruct(this Syntax syntax, StructFormatter parent)
        {
            switch(syntax.TokenClass)
            {
                case List _: return new ListStructure(syntax, parent);
                case Colon _: return new DeclarationStructure(syntax, parent);
            }

            return new ChainStructure(syntax, parent);
        }

        internal static IStructure CreateDeclaratorStruct(this Syntax syntax, StructFormatter parent)
        {
            if(syntax.Left == null && syntax.Right == null)


                return new SingleItemStructure(syntax, parent);

            Dumpable.NotImplementedFunction(syntax, parent);
            return null;
        }

        internal static IStructure CreateListItemStruct(this Syntax syntax, StructFormatter parent)
        {
            switch(syntax.TokenClass)
            {
                case Colon _:
                    Dumpable.NotImplementedFunction(syntax, parent);
                    return null;
            }

            return CreateStruct(syntax, parent);
        }

        /// <summary>
        ///     Calulate the length, if all ignorable linebreaks would have been ignored.
        ///     Spaces outside of comments are always ignored.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="configuration"></param>
        /// <returns>the line length or null if there is any line break, that cannot be ignored.</returns>
        static TContainer FlatFormat<TContainer, TValue>(this Syntax target, Configuration configuration)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = target
                .Token
                .FlatFormat(configuration);

            if(tokenString == null)
                return null;

            tokenString = target.LeftSideSeparator().Text + tokenString + target.RightSideSeparator().Text;

            var leftResult = target.Left.FlatSubFormat<TContainer, TValue>(configuration);
            if(leftResult == null)
                return null;

            var rightResult = target.Right.FlatSubFormat<TContainer, TValue>(configuration);
            if(rightResult == null)
                return null;

            return leftResult.Concat(tokenString, rightResult);
        }

        static TContainer FlatSubFormat<TContainer, TValue>(this Syntax left, Configuration configuration)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null ? new TContainer() : left.FlatFormat<TContainer, TValue>(configuration);

        static string FlatFormat(this IToken target, Configuration configuration)
        {
            var result = "";
            foreach(var item in target.PrecededWith.Where(item => item.IsComment()))
            {
                if(item.HasLines())
                    return null;
                result += item.SourcePart.Id;
            }

            if(configuration.EmptyLineLimit != 0 && target.PrecededWith.Any(item => item.IsLineBreak()))
                return null;

            return result + target.Characters.Id;
        }

        internal static ISeparatorType LeftSideSeparator(this Syntax target)
        {
            var left = target.Left?.RightMost.TokenClass;
            if(target.Token.PrecededWith.HasComment())
                return SeparatorType.ContactSeparator;
            return SeparatorType.Get(left, target.TokenClass);
        }

        internal static ISeparatorType RightSideSeparator(this Syntax target)
        {
            var right = target.Right?.LeftMost;
            if(right == null || right.Token.PrecededWith.HasComment())
                return SeparatorType.ContactSeparator;
            return SeparatorType.Get(target.TokenClass, right.TokenClass);
        }

        internal static string FlatFormat
            (this Syntax target, Configuration configuration)
            => target.FlatFormat<StringResult, string>(configuration).Value;


        internal static bool IsLineBreakRequired(this Syntax syntax, Configuration configuration)
        {
            var basicLineLength = syntax.FlatFormat<IntegerResult, int>(configuration)?.Value;
            return basicLineLength == null || basicLineLength > configuration.MaxLineLength;
        }
    }
}