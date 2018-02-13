using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Parser
{
    sealed class ScannerTokenFactory : DumpableObject, ILexerTokenFactory, IScannerContext
    {
        static IEnumerable<KeyValuePair<string, IMatchProvider>> GetPredefinedMatchProviders(Type factoryType)
            => factoryType.GetBelongings<INamedMatchProvider>().Select(CreateMatchProvider);

        static KeyValuePair<string, IMatchProvider> CreateMatchProvider(INamedMatchProvider provider)
            => new KeyValuePair<string, IMatchProvider>(provider.Value, provider);

        readonly IDictionary<string, IMatchProvider> MatchProviders;
        readonly IMatch ParserLiteralMatch;

        public ScannerTokenFactory
        (
            IDictionary<string, IExpression> scannerDefinitions,
            IMatch parserLiteralMatch
        )
        {
            ParserLiteralMatch = parserLiteralMatch;
            MatchProviders =
                scannerDefinitions
                    .ToDictionary(i => i.Key, i => (IMatchProvider) new BnfMatchProvider(i.Value, this))
                    .Concat(GetPredefinedMatchProviders(GetType()))
                    .ToDictionary(i => i.Key, i => i.Value);
        }


        ITokenType ILexerTokenFactory.EndOfText => new EndOfText();
        ITokenType ILexerTokenFactory.InvalidCharacterError => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ILexerTokenFactory.Classes
        {
            get
            {
                var lexerTokenItems = GetType()
                    .GetBelongings<ILexerTokenType>()
                    .Select(CreateLexerItem);

                var tokenItems = GetType()
                    .GetBelongings<ITokenType>()
                    .Select(CreateLexerItem);

                var parserLiteralItem = new LexerItem
                (
                    new ParserLiteral(),
                    sourcePosition => ParserLiteralMatch.Match(sourcePosition)
                );

                return lexerTokenItems
                    .Concat(tokenItems)
                    .OrderBy(Priority)
                    .Concat(new[] {parserLiteralItem})
                    .ToArray();
            }
        }

        IMatchProvider IScannerContext.Resolve(string name) => MatchProviders[name];

        int Priority(LexerItem item)
        {
            var result = MatchProviders.Keys.IndexWhere(k => k == item.LexerTokenType.Value);
            if(result != null)
                return result.Value;

            NotImplementedFunction(item);
            return 0;
        }

        LexerItem CreateLexerItem(ILexerTokenType type)
            => new LexerItem(type, MatchProviders[type.Value].Function);

        LexerItem CreateLexerItem(ITokenType type)
            => new LexerItem(new ImbeddedTokenType(type), MatchProviders[type.Value].Function);
    }

    sealed class ParserLiteral : DumpableObject, IFactoryTokenType, ITokenType
    {
        string IUniqueIdProvider.Value => "<parserliteral>";
        ITokenType ITokenTypeFactory.Get(string id) => this;
    }

    interface IMatchProvider
    {
        int? Function(SourcePosn sourcePosn);
    }

    interface INamedMatchProvider : IMatchProvider, IUniqueIdProvider {}

    sealed class BnfMatchProvider : DumpableObject, IMatchProvider
    {
        readonly IScannerContext Context;
        readonly IExpression Expression;

        public BnfMatchProvider(IExpression expression, IScannerContext context)
        {
            Expression = expression;
            Context = context;
            Tracer.Assert(Expression != null);
        }

        int? IMatchProvider.Function(SourcePosn sourcePosn)
            => Expression.Match(sourcePosn, Context);
    }

    interface IScannerContext
    {
        IMatchProvider Resolve(string name);
    }

    sealed class ScannerSyntaxError : DumpableObject, ITokenType
    {
        readonly IssueId IssueId;
        public ScannerSyntaxError(IssueId issueId) => IssueId = issueId;

        string IUniqueIdProvider.Value => "<error>";
    }
}