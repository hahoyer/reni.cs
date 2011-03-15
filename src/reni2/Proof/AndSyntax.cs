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
                    .Select(parsedSyntax => parsedSyntax.ToDefinition())
                    .Where(pair => pair != null)
                    .Select(pair => pair.Value);
            }
        }

        internal AndSyntax Isolate() { return Apply(new IsolationStrategy()); }

        private AndSyntax Apply(IStrategy strategy) { return (AndSyntax) Operator.CombineAssosiative(Token, Set | Set.SelectMany(strategy.Apply).ToSet()); }

        internal AndSyntax Replace() { return Apply(new ReplaceStrategy(this)); }
        public string SmartDump() { return SmartDump(null); }
    }

    internal sealed class ReplaceStrategy : ReniObject, IStrategy
    {
        private readonly IEnumerable<KeyValuePair<string, ParsedSyntax>> _definitions;
        internal ReplaceStrategy(AndSyntax andSyntax) { _definitions = andSyntax.Definitions; }

        Set<ParsedSyntax> IStrategy.Apply(ParsedSyntax parsedSyntax)
        {
            return
                parsedSyntax
                    .Replace(_definitions)
                - parsedSyntax;
        }
    }

    internal sealed class IsolationStrategy : ReniObject, IStrategy
    {
        Set<ParsedSyntax> IStrategy.Apply(ParsedSyntax parsedSyntax)
        {
            return parsedSyntax
                .Variables
                .Select(parsedSyntax.IsolateClause)
                .Where(x => x != null)
                .ToSet();
        }
    }

    internal interface IStrategy
    {
        Set<ParsedSyntax> Apply(ParsedSyntax parsedSyntax);
    }
}