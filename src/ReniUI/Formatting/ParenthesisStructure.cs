using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ParenthesisStructure : Structure
    {
        IStructure BodyValue;
        FormatterTokenGroup LeftValue;
        FormatterTokenGroup RightValue;

        public ParenthesisStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterTokenGroup Left => LeftValue ?? (LeftValue = FormatterTokenGroup.Create(Syntax.Left));
        FormatterTokenGroup Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax));

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix)
        {
            if(!exlucdePrefix)
                yield return Left.Prefix;

            yield return Left.Main;

            yield return SourcePartEditExtension.IndentStart.SingleToArray();

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak.SingleToArray();

            yield return Left.Suffix;

            if(Body != null)
                yield return Body.GetSourcePartEdits(targetPart, true);

            yield return Right.Prefix;

            yield return SourcePartEditExtension.IndentEnd.SingleToArray();
            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak.SingleToArray();
            yield return Right.Main;

            yield return Right.Suffix;
        }

        IStructure GetBody()
            => Syntax
                .Left
                .AssertNotNull()
                .Right
                .CreateBodyStruct(Parent, IsLineBreakRequired);
    }
}