using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ParenthesisStructure : StructureBase
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
            var result = new List<ISourcePartEdit>();
            if(!exlucdePrefix)
                result.AddRange(Left.Prefix);

            result.AddRange(Left.Main);

            result.Add(SourcePartEditExtension.IndentStart);

            if(IsLineBreakRequired)
                result.Add(SourcePartEditExtension.LineBreak);

            result.AddRange(Left.Suffix);

            if(Body != null)
                result.AddRange(Body.GetSourcePartEdits(targetPart, true, false));

            result.AddRange(Right.Prefix);

            result.Add(SourcePartEditExtension.IndentEnd);
            if(IsLineBreakRequired)
                result.Add(SourcePartEditExtension.LineBreak);
            result.AddRange(Right.Main);

            result.AddRange(Right.Suffix);
            return result.SingleToArray();
        }

        IStructure GetBody()
            => Syntax
                .Left
                .AssertNotNull()
                .Right
                .CreateBodyStruct(Parent, IsLineBreakRequired);
    }
}