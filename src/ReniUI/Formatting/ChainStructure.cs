using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class ChainStructure : Structure
    {
        public ChainStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent) {}

        protected override IEnumerable<ISourcePartEdit> GetSourcePartEdits(SourcePart targetPart, bool? exlucdePrefix)
        {
            if(Syntax.Left == null && Syntax.Right == null)
            {
                if(exlucdePrefix == true)
                    return Enumerable.Empty<ISourcePartEdit>();

                var tokenGroup = FormatterTokenGroup.Create(Syntax);
                if(exlucdePrefix == null)
                    Tracer.Assert(!tokenGroup.Prefix.Any());

                return tokenGroup.Prefix.Select(item => item.ToSourcePartEdit());
            }

            NotImplementedMethod(targetPart, exlucdePrefix);
            return null;
        }
    }
}