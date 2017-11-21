using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
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

            Dumpable.NotImplementedFunction(syntax, parent);
            return null;
        }

        internal static int? EssentialLineLength(this Syntax target)
        {
            var result = target.Token.EssentialLineLength();
            if(result == null)
                return null;

            var leftResult = target.Left?.EssentialLineLength();
            if(leftResult == null)
                return null;

            var rightResult = target.Right?.EssentialLineLength();
            if(rightResult == null)
                return null;

            return leftResult + result + rightResult;
        }

        static int? EssentialLineLength(this IToken target)
        {
            var result = 0;
            foreach(var item in target.PrecededWith)
                if(item.IsComment())
                {
                    if(item.HasLines())
                        return null;
                    result += item.SourcePart.Length;
                }

            return result + target.Characters.Length;
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
        static int? BasicLineLength(this Syntax target, Configuration configuration)
        {
            var leftTokenClass = target.Left?.RightMost.TokenClass;
            var rightNeighbor = target.Right?.LeftMost;
            if(rightNeighbor != null && rightNeighbor.Token.PrecededWith.HasComment())
                rightNeighbor = null;
            var rightTokenClass = rightNeighbor?.TokenClass;

            var tokenLength = target.Token.BasicLineLength
                (leftTokenClass, target.TokenClass, rightTokenClass, configuration);
            if(tokenLength == null)
                return null;

            var leftLength = target.Left?.BasicLineLength(configuration);
            if(leftLength == null)
                return null;
            var rightLength = target.Right?.BasicLineLength(configuration);
            if(rightLength == null)
                return null;
            return leftLength + tokenLength + rightLength;
        }

        static int? BasicLineLength
        (
            this IToken target,
            ITokenClass leftTokenClass,
            ITokenClass targetTokenClass,
            ITokenClass rightTokenClass,
            Configuration configuration)
        {
            var result = 0;
            foreach(var item in target.PrecededWith.Where(item => item.IsComment()))
            {
                if(item.HasLines())
                    return null;
                result += item.SourcePart.Length;
            }

            if(configuration.EmptyLineLimit != 0 && target.PrecededWith.Any(item => item.IsLineBreak()))
                return null;

            if(result == 0 && leftTokenClass != null)
                result += SeparatorType.Get(leftTokenClass, targetTokenClass).Text.Length;

            if(rightTokenClass != null)
                result += SeparatorType.Get(targetTokenClass, rightTokenClass).Text.Length;

            return result + target.Characters.Length;
        }

        internal static bool IsLineBreakRequired(this Syntax syntax, Configuration configuration)
        {
            var basicLineLength = syntax.BasicLineLength(configuration);
            if(basicLineLength == null)
                return true;
            return basicLineLength > configuration.MaxLineLength;
        }
    }
}