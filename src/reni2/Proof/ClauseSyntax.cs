using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class ClauseSyntax : AssociativeSyntax
    {
        public ClauseSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(@operator, token, set) {}

        public IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(int variableCount)
        {
            return Set
                .Where(parsedSyntax=>parsedSyntax.Variables.Count() <= variableCount)
                .SelectMany(GetDefinitions)
                .Where(pair => pair.Value != null);
        }

        private static IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(ParsedSyntax parsedSyntax) { return parsedSyntax.Variables.Select(parsedSyntax.GetDefinition); }

        public string SmartDump() { return SmartDump(null); }
        public ClauseSyntax IsolateAndReplace(int variableCount) { return Apply(new IsolateAndReplaceStrategy(this,variableCount)); }

        private ClauseSyntax Apply(IStrategy strategy)
        {
            return (ClauseSyntax) 
                   Operator
                       .CombineAssosiative(Token, Set | Set.SelectMany(strategy.Apply)
                                                            .ToSet());
        }
    }
}