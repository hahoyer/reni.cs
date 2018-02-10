using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem> : DumpableObject, PrioTable.ITargetItem, IPrioParserToken
        where TTreeItem : class, ISourcePartProxy
    {
        internal static Item<TTreeItem> Create
        (
            IEnumerable<LexerToken> prefixItems,
            IPriorityParserTokenType<TTreeItem> parserType,
            SourcePart characters,
            BracketContext context,
            bool? isBracketAndLeftBracket)
            =>
                new Item<TTreeItem>
                (
                    prefixItems,
                    parserType,
                    characters,
                    context,
                    isBracketAndLeftBracket);

        internal static Item<TTreeItem> Create(TokenGroup items, BracketContext context)
        {
            var isBracketAndLeftBracket = context.IsBracketAndLeftBracket(items.Characters.Id);
            var parserType = (IPriorityParserTokenType<TTreeItem>) items.Type;

            return new Item<TTreeItem>
            (
                items.PrefixItems,
                parserType,
                items.Characters,
                context,
                isBracketAndLeftBracket);
        }

        internal static Item<TTreeItem> CreateStart
        (
            Source source,
            PrioTable prioTable,
            IPriorityParserTokenType<TTreeItem> startParserType)
            =>
                new Item<TTreeItem>
                (
                    new LexerToken[0],
                    startParserType,
                    (source + 0).Span(0),
                    prioTable.BracketContext,
                    null);

        internal readonly SourcePart Characters;

        [DisableDump]
        internal readonly BracketContext Context;

        [EnableDumpExcept(null)]
        internal readonly bool? IsBracketAndLeftBracket;

        [DisableDump]
        internal readonly LexerToken[] PrefixItems;

        internal readonly IPriorityParserTokenType<TTreeItem> Type;

        Item
        (
            IEnumerable<LexerToken> prefixItems,
            IPriorityParserTokenType<TTreeItem> type,
            SourcePart characters,
            BracketContext context,
            bool? isBracketAndLeftBracket)
        {
            PrefixItems = prefixItems.ToArray();
            Type = type;
            Characters = characters;
            Context = context;
            IsBracketAndLeftBracket = isBracketAndLeftBracket;
        }

        IEnumerable<LexerToken> IToken.PrecededWith => PrefixItems;
        SourcePart IToken.Characters => Characters;
        bool? IPrioParserToken.IsBracketAndLeftBracket => IsBracketAndLeftBracket;

        string PrioTable.ITargetItem.Token => Type?.Value ?? PrioTable.BeginOfText;
        BracketContext PrioTable.ITargetItem.LeftContext => Context;

        [EnableDump]
        internal int Depth => Context?.Depth ?? 0;

        internal Item<TTreeItem> RecreateWith
        (
            IEnumerable<LexerToken> newPrefixItems = null,
            IPriorityParserTokenType<TTreeItem> newType = null,
            BracketContext newContext = null)
            => new Item<TTreeItem>
            (
                newPrefixItems ?? PrefixItems,
                newType ?? Type,
                Characters,
                newContext ?? Context,
                IsBracketAndLeftBracket);

        internal TTreeItem Create(TTreeItem left) => Type.Create(left, this, null);
    }
}