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
                case Colon _:
                    Dumpable.NotImplementedFunction(syntax, parent);
                    return null;
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
            if (rightResult == null)
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
    }
}