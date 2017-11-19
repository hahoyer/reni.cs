using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    class ParenthesisStructure : DumpableObject, IStructure
    {
        readonly StructFormatter Parent;

        [EnableDump]
        readonly Syntax Syntax;

        IStructure BodyValue;
        FormatterToken[] LeftValue;
        FormatterToken[] RightValue;

        public ParenthesisStructure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
        {
            Tracer.Assert(Left.Length == 1);
            Tracer.Assert(Right.Length == 1);

            yield return Left.Single().ToSourcePartEdit();
            yield return SourcePartEditExtension.IndentStart;

            foreach(var edit in Body.GetSourcePartEdits(targetPart))
                yield return edit;

            if(HasInnerLineBreak)
                yield return SourcePartEditExtension.LineBreak;

            yield return SourcePartEditExtension.IndentEnd;
            yield return Right.Single().ToSourcePartEdit();
        }

        bool IStructure.LineBreakScan(ref int? lineLength) => LineBreakScan(ref lineLength);

        IStructure Body => BodyValue ?? (BodyValue = GetBody());
        FormatterToken[] Left => LeftValue ?? (LeftValue = FormatterToken.Create(Syntax.Left).ToArray());
        FormatterToken[] Right => RightValue ?? (RightValue = FormatterToken.Create(Syntax).ToArray());

        bool HasInnerLineBreak
        {
            get
            {
                var lineLength = Parent.Configuration.MaxLineLength;
                return LineBreakScan(ref lineLength);
            }
        }

        IStructure GetBody()
            => Syntax
                .Left
                .AssertNotNull()
                .Right
                .AssertNotNull()
                .CreateStruct(Parent);

        bool LineBreakScan(ref int? lineLength)
            => Left.LineBreakScan(ref lineLength) ||
               Body.LineBreakScan(ref lineLength) ||
               Right.LineBreakScan(ref lineLength);
    }
}