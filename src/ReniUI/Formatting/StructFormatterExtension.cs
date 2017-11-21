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

        internal static int? BasicLineLength(this Syntax target)
        {
            var tokenLength = target.Token.BasicLineLength();
            if(tokenLength == null)
                return null;
            var leftLength = target.Left?.BasicLineLength();
            if(leftLength == null)
                return null;
            var rightLength = target.Right?.BasicLineLength();
            if(rightLength == null)
                return null;
            return leftLength + tokenLength + rightLength;
        }

        static int? BasicLineLength(this IToken target)
        {
            var result = 0;
            foreach(var item in target.PrecededWith.Where(item => item.IsComment()))
            {
                if(item.HasLines())
                    return null;
                result += item.SourcePart.Length;
            }

            return result + target.Characters.Length;
        }

        internal static bool HasLineBreak(this Syntax syntax, Configuration configuration)
        {
            var x = syntax.BasicLineLength();
            if(x == null)
                return true;
            return x > configuration.MaxLineLength;
        }
    }
}