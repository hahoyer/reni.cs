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
        readonly ValueCache<bool> IsLineBreakRequiredCache;
        IStructure BodyValue;
        FormatterToken[] LeftValue;
        FormatterToken[] RightValue;

        public ParenthesisStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent)
        {
            IsLineBreakRequiredCache = new ValueCache<bool>(() => Syntax.IsLineBreakRequired(Parent.Configuration));
        }

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterToken[] Left => LeftValue ?? (LeftValue = FormatterToken.Create(Syntax.Left).ToArray());
        FormatterToken[] Right => RightValue ?? (RightValue = FormatterToken.Create(Syntax).ToArray());

        bool IsLineBreakRequired => IsLineBreakRequiredCache.Value;

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart)
        {
            Tracer.Assert(Left.Length == 1);
            Tracer.Assert(Right.Length == 1);

            yield return Left.Single().ToSourcePartEdit();
            yield return SourcePartEditExtension.IndentStart;

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;

            foreach(var edit in Body.GetSourcePartEdits(targetPart))
                yield return edit;

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;

            yield return SourcePartEditExtension.IndentEnd;
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