using System.Collections.Generic;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class DeclarationStructure : StructureBase
    {
        static bool IsMultiLine => false;
        static bool IsInnerMultiLine => false;
        IStructure LeftValue;
        IStructure RightValue;

        public DeclarationStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        IStructure Left => LeftValue ?? (LeftValue = GetLeft());
        IStructure Right => RightValue ?? (RightValue = GetRight());

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            var result = new List<ISourcePartEdit>();
            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentEnd);

            result.AddRange(Left.GetSourcePartEdits(targetPart, exlucdePrefix, false));

            var declarationToken = FormatterTokenGroup.Create(Syntax);
            result.AddRange(declarationToken.Prefix);
            result.AddRange(declarationToken.Main);
            result.AddRange(declarationToken.Suffix);

            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentStart);

            if(IsMultiLine && IsInnerMultiLine)
                result.Add(SourcePartEditExtension.LineBreak);

            var sourcePartEdits = Right.GetSourcePartEdits(targetPart, false, false);
            result.AddRange(sourcePartEdits);

            if(IsMultiLine)
                result.Add(SourcePartEditExtension.IndentEnd);
            return result.SingleToArray();
        }

        IStructure GetLeft() => Syntax.Left.AssertNotNull().CreateDeclaratorStruct(Parent);
        IStructure GetRight() => Syntax.Right.AssertNotNull().CreateStruct(Parent);
    }

    sealed class DeclaratorItemStructure : StructureBase
    {
        public DeclaratorItemStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}


        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            var result = new List<ISourcePartEdit>();

            if(Syntax.Left != null)
            {
                var edits = Syntax
                    .Left
                    .CreateDeclarationTagStruct(Parent)
                    .GetSourcePartEdits(targetPart, exlucdePrefix, false);
                result.AddRange(edits);
                exlucdePrefix = true;
            }

            var tokenGroup = FormatterTokenGroup.Create(Syntax);
            if(!exlucdePrefix)
                result.AddRange(tokenGroup.Prefix);
            result.AddRange(tokenGroup.Main);
            return result.SingleToArray();
        }
    }

    sealed class DeclarationTagStructure : StructureBase
    {
        public DeclarationTagStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}


        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits
            (SourcePart targetPart, bool exlucdePrefix)
        {
            var l = Syntax.Left;
            var r = Syntax.Right;

            NotImplementedMethod(targetPart, exlucdePrefix);
            return null;
        }
    }
}