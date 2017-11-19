using hw.DebugFormatter;
using JetBrains.Annotations;
using NUnit.Framework.Api;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        [CanBeNull]
        internal static IStructure CreateStruct(this Syntax syntax, StructFormatter parent)
        {
            if(syntax.TokenClass is RightParenthesis)
                return new ParenthesisStructure(syntax, parent);

            if(syntax.TokenClass is List)
                return new ListStructure(syntax, parent);

            if(syntax.TokenClass is Number)
            {
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
    }
}