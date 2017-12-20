using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;


namespace ReniUI.Formatting
{
    sealed class ParenthesisStructure : Structure
    {
        IStructure BodyValue;
        FormatterTokenGroup LeftValue;
        FormatterTokenGroup RightValue;

        public ParenthesisStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent)
        {
        }

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterTokenGroup Left => LeftValue ?? (LeftValue = FormatterTokenGroup.Create(Syntax.Left));
        FormatterTokenGroup Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax));

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            foreach(var edit in Left.Prefix)
                yield return edit;

            yield return Left.Main;


            yield return SourcePartEditExtension.IndentStart;

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;

            foreach (var edit in Left.Suffix)
                yield return edit;


            foreach (var edit in Body.GetSourcePartEdits(targetPart))
                yield return edit;

            foreach(var edit in Right.Prefix)
            {
                if (IsLineBreakRequired)
                    yield return SourcePartEditExtension.LineBreak;

                yield return edit;
            }

            yield return SourcePartEditExtension.IndentEnd;
            if (IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;
            yield return Right.Main;

            foreach (var edit in Right.Suffix)
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