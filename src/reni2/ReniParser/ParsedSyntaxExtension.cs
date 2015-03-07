using System.Linq;
using System.Collections.Generic;
using System;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.ReniParser
{
    static class ParsedSyntaxExtension
    {
        internal static CompileSyntax CheckedToCompiledSyntax
            (this Syntax parsedSyntax, SourcePart token, Func<IssueId> getError)
        {
            if(parsedSyntax == null)
                return new CompileSyntaxError(getError(), token, null);
            return parsedSyntax.ToCompiledSyntax;
        }
    }
}