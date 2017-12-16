using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
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
                var tokenGroup = FormatterTokenGroup.Create(Syntax);
                if(exlucdePrefix != true)
                    foreach(var edit in tokenGroup.Prefix)
                        yield return edit;

                yield return tokenGroup.Main;
                yield break;
            }

            NotImplementedMethod(targetPart, exlucdePrefix);
            ;
        }
    }
}