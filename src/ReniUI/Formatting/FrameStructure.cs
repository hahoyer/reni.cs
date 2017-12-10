using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class FrameStructure : Structure
    {
        IStructure BodyValue;
        FormatterTokenGroup RightValue;

        public FrameStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterTokenGroup Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax));

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            return
                Body.GetSourcePartEdits(targetPart, exlucdePrefix)
                    .Concat(Right.FormatFrameEnd(Parent.Configuration));
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