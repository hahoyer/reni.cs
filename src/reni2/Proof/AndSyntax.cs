using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class AndSyntax : AssociativeSyntax
    {
        public AndSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { }

        public IEnumerable<KeyValuePair<string, ParsedSyntax>> Definitions
        {
            get
            {
                return Set
                    .SelectMany(GetDefinitions)
                    .Where(pair => pair.Value != null);
            }
        }

        private static IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(ParsedSyntax parsedSyntax) { return parsedSyntax.Variables.Select(parsedSyntax.GetDefinition); }

        private AndSyntax Apply(IStrategy strategy) { return (AndSyntax) Operator.CombineAssosiative(Token, Set | Set.SelectMany(strategy.Apply).ToSet()); }

        public string SmartDump() { return SmartDump(null); }
        public AndSyntax IsolateAndReplace() { return Apply(new IsolateAndReplaceStrategy(this)); }
    }

    internal sealed class IsolateAndReplaceStrategy : ReniObject, IStrategy
    {
        private readonly IEnumerable<KeyValuePair<string, ParsedSyntax>> _definitions;
        internal IsolateAndReplaceStrategy(AndSyntax andSyntax) { _definitions = andSyntax.Definitions; }

        Set<ParsedSyntax> IStrategy.Apply(ParsedSyntax parsedSyntax)
        {
            var trace = false;
            StartMethodDump(trace,parsedSyntax);
            return ReturnMethodDump(trace,
                parsedSyntax
                    .Replace(_definitions)
                - parsedSyntax);
        }
    }

    internal interface IStrategy
    {
        Set<ParsedSyntax> Apply(ParsedSyntax parsedSyntax);
    }
}