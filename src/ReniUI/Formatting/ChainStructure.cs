using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    class ChainStructure : DumpableObject, IStructure
    {
        readonly StructFormatter Parent;
        readonly Syntax Syntax;

        public ChainStructure(Syntax syntax, StructFormatter parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        [CanBeNull]
        IEnumerable<ISourcePartEdit> IStructure.GetSourcePartEdits(SourcePart targetPart)
        {
            if(Syntax.Left == null && Syntax.Right == null)
                return FormatterToken.Create(Syntax).Select(item => item.ToSourcePartEdit());

            NotImplementedMethod(targetPart);
            return null;
        }

        public bool LineBreakScan(ref int? lineLength)
        {
            if(Syntax.Left == null && Syntax.Right == null)
            {
                var tokens = FormatterToken.Create(Syntax).ToArray();
                Tracer.Assert(tokens.Length == 1);
                var token = tokens.Single();
                return token.LineBreakScan(ref lineLength);
            }

            NotImplementedMethod(lineLength);
            return false;
        }
    }
}