using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class DeclarationStructure : Structure
    {
        IStructure LeftValue;
        IStructure RightValue;

        public DeclarationStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Left => LeftValue ?? (LeftValue = GetLeft());
        IStructure Right => RightValue ?? (RightValue = GetRight());

        static bool IsMultiLine => false;
        static bool IsInnerMultiLine => false;

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentEnd;

            foreach(var edit in Left.GetSourcePartEdits(targetPart, exlucdePrefix))
                yield return edit;

            var declarationToken = FormatterTokenGroup.Create(Syntax);
            foreach(var edit in declarationToken.Prefix)
                yield return edit;
            if (Syntax.LeftSideSeparator() == SeparatorType.CloseSeparator)
                yield return SourcePartEditExtension.Space;
            yield return declarationToken.Main;

            foreach (var edit in declarationToken.Suffix)
                yield return edit;

            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentStart;

            if(IsMultiLine && IsInnerMultiLine)
                yield return SourcePartEditExtension.LineBreak;
            else if(Syntax.RightSideSeparator() == SeparatorType.CloseSeparator)
                yield return SourcePartEditExtension.Space;

            foreach(var edit in Right.GetSourcePartEdits(targetPart, false))
                yield return edit;

            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentEnd;
        }

        IStructure GetLeft() => Syntax.Left.AssertNotNull().CreateDeclaratorStruct(Parent);
        IStructure GetRight() => Syntax.Right.AssertNotNull().CreateBodyStruct(Parent);
    }
}