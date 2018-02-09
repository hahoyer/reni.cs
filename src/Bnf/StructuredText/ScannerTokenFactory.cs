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
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<ISyntax>
    {
        static IScannerTokenType CreateInstance(Type type)
            => (IScannerTokenType) Activator.CreateInstance(type);

        IParserTokenType<ISyntax> ITokenFactory<ISyntax>.BeginOfText => new BeginOfText();
        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();
        IScannerTokenType ITokenFactory.InvalidCharacterError => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;
        LexerItem[] Classes => ToLexerItems(Bnf.Compiler.FromText(BnfDefinitions.Scanner).Statements);

        LexerItem[] ToLexerItems(IDictionary<string, IExpression> statements)
        {
            var names = statements.Keys;
            var scannerContext = new ScannerContext(statements);
            return GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory<IScannerTokenType>)
                .Select(scannerContext.CreateItem)
                .OrderBy(scannerContext.Priority)
                .ToArray();
        }


        bool BelongsToFactory<T>(Type type)
        {
            var thisType = GetType();
            return type.Is<T>() &&
                   !type.IsAbstract &&
                   type
                       .GetAttributes<BelongsToAttribute>(true)
                       .Any(attr => thisType.Is(attr.TokenFactory));
        }
    }

    sealed class ScannerContext : DumpableObject
    {
        readonly IDictionary<string, IExpression> Statements;
        public ScannerContext(IDictionary<string, IExpression> statements) => Statements = statements;

        public int Priority(LexerItem item)
        {
            var result = Statements.Keys.IndexWhere(k => k == item.ScannerTokenType.Id);
            if(result != null)
                return result.Value;

            NotImplementedFunction(item);
            return 0;
        }

        public LexerItem CreateItem(Type type)
        {
            var tokenType = (IScannerTokenType) Activator.CreateInstance(type);
            Statements.TryGetValue(tokenType.Id, out var value);
            var matchProvider = value == null ? (IMatchProvider) tokenType : new BnfMatchProvider(value, this);
            return new LexerItem(tokenType, matchProvider.Function);
        }

        public IMatchProvider Resolve(string name)
        {
            Statements.TryGetValue(name, out var expression);
            if(expression != null)
                return new BnfMatchProvider(expression, this);
        }
    }

    interface IMatchProvider
    {
        int? Function(SourcePosn sourcePosn);
    }

    sealed class BnfMatchProvider : DumpableObject, IMatchProvider
    {
        readonly ScannerContext Context;
        readonly IExpression Expression;

        public BnfMatchProvider(IExpression expression, ScannerContext context)
        {
            Expression = expression;
            Context = context;
        }

        int? IMatchProvider.Function(SourcePosn sourcePosn)
            => Expression.Match(sourcePosn, Context);
    }
}