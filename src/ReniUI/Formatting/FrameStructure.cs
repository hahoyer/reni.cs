using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FrameStructure : Structure
    {
        IStructure BodyValue;
        FormatterToken[] RightValue;

        public FrameStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterToken[] Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax).Prefix.ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            foreach(var edit in Body.GetSourcePartEdits(targetPart, exlucdePrefix))
                yield return edit;

            if(Syntax.IsLineBreakRequired(Parent.Configuration))
                yield return SourcePartEditExtension.LineBreak;
        }

        IStructure GetBody()
            => Syntax
                .Left
                .AssertNotNull()
                .Right
                .AssertNotNull()
                .CreateBodyStruct(Parent);
    }
}