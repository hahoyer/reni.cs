using System.Collections;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    static class StructFormatterExtension
    {
        internal interface IStructure
        {
            IEnumerable<SourcePartEdit> GetSourcePartEdits(SourcePart targetPart);
        }

        class ParenthesisStructure : DumpableObject, IStructure
        {
            [EnableDump]
            readonly Syntax Syntax;

            public ParenthesisStructure(Syntax syntax) => Syntax = syntax;

            IEnumerable<SourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
            {
                if(HasOuterLinebreak)
                    yield return new SourcePartEdit(Syntax.Token.PrecededWith.SourcePart(), "\n");
                yield return new SourcePartEdit(Syntax.Token.Characters);
                if(HasInnerLineBreak)
                    yield return new SourcePartEdit(Syntax.Token.Characters.End.Span(0), "\n");

                foreach(var edit in GetInnerSourcePartEdits(targetPart))
                    yield return edit;

                if(HasInnerLineBreak)
                    yield return new SourcePartEdit(Syntax.Left.Right.Token.SourcePart().Start.Span
                        , "\n");
                else
                    yield return new SourcePartEdit(Syntax.Left.Right.Token.Characters);

                if(HasOuterLinebreak)
                    yield return new SourcePartEdit(Syntax.Token.PrecededWith.SourcePart(), "\n");

            }

            IEnumerable<SourcePartEdit> GetInnerSourcePartEdits(SourcePart targetPart)
            {
                
            }

            bool HasInnerLineBreak
                {get {throw new System.NotImplementedException();}}

            bool HasOuterLinebreak
                {get {throw new System.NotImplementedException();}}
        }

        internal static IStructure CreateStruct(this Syntax syntax)
        {
            if(syntax.TokenClass is RightParenthesis)
                return new ParenthesisStructure(syntax);
            Dumpable.NotImplementedFunction(syntax);
            return null;
        }
    }
}