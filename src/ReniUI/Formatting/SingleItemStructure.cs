using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class SingleItemStructure : Structure
    {
        public SingleItemStructure(Syntax syntax, StructFormatter parent)
            : base(syntax, parent)
        {
            Tracer.Assert(Syntax.Left == null);
            Tracer.Assert(Syntax.Right == null);
        }

        protected override IEnumerable<IEnumerable<ISourcePartEdit>> GetSourcePartEdits(SourcePart targetPart, bool exlucdePrefix)
        {
            var tokenGroup = FormatterTokenGroup.Create(Syntax);
            if(!exlucdePrefix)
                Tracer.Assert(!tokenGroup.Prefix.Any());

            yield return tokenGroup.Prefix;
            yield return tokenGroup.Main;
        }
    }
}