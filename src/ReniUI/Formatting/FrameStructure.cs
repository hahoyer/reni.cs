using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    class FrameStructure : Structure
    {
        IStructure BodyValue;
        FormatterToken[] RightValue;

        public FrameStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterToken[] Right => RightValue ?? (RightValue = FormatterToken.Create(Syntax).ToArray());

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart)
        {
            Tracer.Assert(Right.Length == 1);

            foreach(var edit in Body.GetSourcePartEdits(targetPart))
                yield return edit;

            if(Syntax.IsLineBreakRequired(Parent.Configuration))
                yield return SourcePartEditExtension.LineBreak;

            yield return Right.Single().ToSourcePartEdit();
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