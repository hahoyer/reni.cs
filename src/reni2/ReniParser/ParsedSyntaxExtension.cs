using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.ReniParser
{
    static class ParsedSyntaxExtension
    {
        internal static CompileSyntax CheckedToCompiledSyntax
            (this Syntax parsedSyntax, IToken token, Func<IssueId> getError)
        {
            if(parsedSyntax == null)
                return new CompileSyntaxError(getError(), token);
            return parsedSyntax.ToCompiledSyntax;
        }
    }
}