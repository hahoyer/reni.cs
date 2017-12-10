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
        FormatterTokenGroup LeftValue;
        FormatterTokenGroup RightValue;

        public ParenthesisStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent)
        {
            IsLineBreakRequiredCache = new ValueCache<bool>(() => Syntax.IsLineBreakRequired(Parent.Configuration));
        }

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterTokenGroup Left => LeftValue ?? (LeftValue = FormatterTokenGroup.Create(Syntax.Left));
        FormatterTokenGroup Right => RightValue ?? (RightValue = FormatterTokenGroup.Create(Syntax));

        bool IsLineBreakRequired => IsLineBreakRequiredCache.Value;

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            Tracer.Assert(!Left.Prefix.Any());
            Tracer.Assert(!Left.Suffix.Any());
            Tracer.Assert(!Right.Prefix.Any());
            Tracer.Assert(!Right.Suffix.Any());

            yield return SourcePartEditExtension.IndentStart;

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;

            foreach(var edit in Body.GetSourcePartEdits(targetPart))
                yield return edit;

            if(IsLineBreakRequired)
                yield return SourcePartEditExtension.LineBreak;

            yield return SourcePartEditExtension.IndentEnd;
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