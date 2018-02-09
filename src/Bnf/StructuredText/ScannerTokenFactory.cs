using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<ISyntax>, IScannerContext
    {
        static IEnumerable<KeyValuePair<string, IMatchProvider>> GetPredefinedMatchProviders(Type factoryType)
        {
            return
                factoryType
                    .Assembly
                    .GetTypes()
                    .Where(type => type.IsBelongingTo<INamedMatchProvider>(factoryType))
                    .Select(CreateMatchProvider);
        }

        static KeyValuePair<string, IMatchProvider> CreateMatchProvider(Type type)
        {
            var tokenType = (INamedMatchProvider) Activator.CreateInstance(type);
            return new KeyValuePair<string, IMatchProvider>(tokenType.Value, tokenType);
        }

        readonly IDictionary<string, IMatchProvider> MatchProviders;

        public ScannerTokenFactory()
        {
            var statements = Bnf.Compiler.FromText(BnfDefinitions.Scanner).Statements;
            MatchProviders =
                statements.ToDictionary(i => i.Key, i => (IMatchProvider) new BnfMatchProvider(i.Value, this))
                    .Concat(GetPredefinedMatchProviders(GetType()))
                    .ToDictionary(i => i.Key, i => i.Value);
        }

        IMatchProvider IScannerContext.Resolve(string name) => MatchProviders[name];
        IParserTokenType<ISyntax> ITokenFactory<ISyntax>.BeginOfText => new BeginOfText();
        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();
        IScannerTokenType ITokenFactory.InvalidCharacterError => new ScannerSyntaxError(IssueId.InvalidCharacter);
        LexerItem[] ITokenFactory.Classes => GetLexerItems(GetType());

        LexerItem[] GetLexerItems(Type factoryType)
            => factoryType
                .Assembly
                .GetTypes()
                .Where(type => type.IsBelongingTo<IScannerTokenType>(factoryType))
                .Select(CreateLexerItem)
                .OrderBy(Priority)
                .ToArray();

        int Priority(LexerItem item)
        {
            var result = MatchProviders.Keys.IndexWhere(k => k == item.ScannerTokenType.Id);
            if(result != null)
                return result.Value;

            NotImplementedFunction(item);
            return 0;
        }

        LexerItem CreateLexerItem(Type type)
        {
            var tokenType = (IScannerTokenType) Activator.CreateInstance(type);
            return new LexerItem(tokenType, MatchProviders[tokenType.Id].Function);
        }
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