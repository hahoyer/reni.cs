using System.Collections.Generic;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FrameStructure : StructureBase
    {
        IStructure BodyValue;
        FormatterTokenGroup RightValue;

        public FrameStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterTokenGroup Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax));

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            yield return Body.GetSourcePartEdits(targetPart, exlucdePrefix, false);
            foreach(var edit in Right.FormatFrameEnd())
                yield return edit;
        }

        IStructure GetBody()
            => Syntax
                .Left
                .AssertNotNull()
                .Right
                .AssertNotNull()
                .CreateBodyStruct(Parent, IsLineBreakRequired);
    }
}