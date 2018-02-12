using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    sealed class ScannerTokenFactory : DumpableObject, ILexerTokenFactory, IScannerContext
    {
        static IEnumerable<KeyValuePair<string, IMatchProvider>> GetPredefinedMatchProviders(Type factoryType)
            => factoryType.GetBelongings<INamedMatchProvider>().Select(CreateMatchProvider);

        static KeyValuePair<string, IMatchProvider> CreateMatchProvider(INamedMatchProvider provider)
            => new KeyValuePair<string, IMatchProvider>(provider.Value, provider);

        readonly IDictionary<string, IMatchProvider> MatchProviders;

        public ScannerTokenFactory(IDictionary<string, IExpression> definitionTextStatements)
        {
            MatchProviders =
                Enumerable.ToDictionary<KeyValuePair<string, IMatchProvider>, string, IMatchProvider>(
                        definitionTextStatements.ToDictionary(i => i.Key, i => (IMatchProvider) new BnfMatchProvider(i.Value, this))
                            .Concat(GetPredefinedMatchProviders(GetType())), i => i.Key, i => i.Value);
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

                return lexerTokenItems
                    .Concat(tokenItems)
                    .OrderBy(Priority)
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
}