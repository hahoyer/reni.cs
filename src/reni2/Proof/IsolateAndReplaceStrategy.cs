using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;

namespace Reni.Proof
{
    internal sealed class IsolateAndReplaceStrategy : ReniObject, IStrategy
    {
        private readonly int _variableCount;
        private readonly IEnumerable<KeyValuePair<string, ParsedSyntax>> _definitions;
        internal IsolateAndReplaceStrategy(ClauseSyntax clauseSyntax, int variableCount)
        {
            _variableCount = variableCount;
            _definitions = clauseSyntax.GetDefinitions(variableCount);
        }

        Set<ParsedSyntax> IStrategy.Apply(ParsedSyntax parsedSyntax)
        {
            if (parsedSyntax.Variables.Count() > _variableCount)
                return Set<ParsedSyntax>.Empty;
            var trace = false;
            StartMethodDump(trace,parsedSyntax);
            return ReturnMethodDump(trace,
                                    parsedSyntax
                                        .Replace(_definitions)
                                    - parsedSyntax);
        }
    }
}