using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class DeclarationStructure : Structure
    {
        IStructure LeftValue;
        IStructure RightValue;

        public DeclarationStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Left => LeftValue ?? (LeftValue = GetLeft());
        IStructure Right => RightValue ?? (RightValue = GetRight());

        static bool IsMultiLine => false;
        static bool IsInnerMultiLine => false;

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix)
        {
            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentEnd.SingleToArray();

            yield return Left.GetSourcePartEdits(targetPart, exlucdePrefix);

            var declarationToken = FormatterTokenGroup.Create(Syntax);
            yield return declarationToken.Prefix;
            yield return declarationToken.Main;
            yield return declarationToken.Suffix;

            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentStart.SingleToArray();

            if(IsMultiLine && IsInnerMultiLine)
                yield return SourcePartEditExtension.LineBreak.SingleToArray();

            yield return Right.GetSourcePartEdits(targetPart, false);

            if(IsMultiLine)
                yield return SourcePartEditExtension.IndentEnd.SingleToArray();
        }

        IStructure GetLeft() => Syntax.Left.AssertNotNull().CreateDeclaratorStruct(Parent);
        IStructure GetRight() => Syntax.Right.AssertNotNull().CreateBodyStruct(Parent, IsMultiLine);
    }
}