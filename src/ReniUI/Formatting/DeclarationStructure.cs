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
            var result = new List<ISourcePartEdit>();
            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentEnd);

            result.AddRange(Left.GetSourcePartEdits(targetPart, exlucdePrefix));

            var declarationToken = FormatterTokenGroup.Create(Syntax);
            result.AddRange(declarationToken.Prefix);
            result.AddRange(declarationToken.Main);
            result.AddRange(declarationToken.Suffix);

            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentStart);

            if(IsMultiLine && IsInnerMultiLine)
                result.Add(SourcePartEditExtension.LineBreak);

            var sourcePartEdits = Right.GetSourcePartEdits(targetPart, false);
            result.AddRange(sourcePartEdits);

            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentEnd);
            return result.SingleToArray();
        }

        IStructure GetLeft() => Syntax.Left.AssertNotNull().CreateDeclaratorStruct(Parent);
        IStructure GetRight() => Syntax.Right.AssertNotNull().CreateBodyStruct(Parent, IsMultiLine);
    }
}