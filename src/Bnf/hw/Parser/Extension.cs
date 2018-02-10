using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public static class Extension
    {
        public static TOut Operation<TIn, TOut>
            (this IOperator<TIn, TOut> @operator, TIn left, IToken token, TIn right)
            where TIn : class => left == null
            ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right))
            : (right == null
                ? @operator.Suffix(left, token)
                : @operator.Infix(left, token, right));

        public static ISubParser<TTreeItem> Convert<TTreeItem>
            (this IPriorityParser<TTreeItem> parser, Func<TTreeItem, IPriorityParserTokenType<TTreeItem>> converter)
            where TTreeItem : class, ISourcePartProxy
            => new SubParser<TTreeItem>(parser, converter);

        internal static string TreeDump<TTreeItem>(TTreeItem value)
            where TTreeItem : class
            => value is IBinaryTreeItem t ? TreeDump(t) : Tracer.Dump(value);

        public static string TreeDump(this IBinaryTreeItem value)
        {
            if(value == null)
                return "<null>";

            var result = "(";
            result += TreeDump(value.Left);
            result += " ";
            result += value.TokenId;
            result += " ";
            result += TreeDump(value.Right);
            result += ")";
            return result;
        }

        public static SourcePart SourcePart(this IEnumerable<LexerToken> items)
            => items.Select(item => item.SourcePart).Aggregate();

        public static int BracketBalance(this IPrioParserToken token)
        {
            switch(token.IsBracketAndLeftBracket)
            {
                case true: return -1;
                case false: return 1;
                default: return 0;
            }
        }

        internal static BracketContext GetRightContext(this PrioTable.ITargetItem item)
            => item.LeftContext.Add(item.Token);

        internal static int GetRightDepth(this PrioTable.ITargetItem item)
            => item.GetRightContext().Depth;

        internal static int GetLeftDepth(this PrioTable.ITargetItem item)
            => item.LeftContext.Depth;


        static bool IsBelongingTo<T>(this Type childType, Type factoryType)
            => childType.Is<T>() &&
               !childType.IsAbstract &&
               childType
                   .GetAttributes<BelongsToAttribute>(true)
                   .Any(attr => factoryType.Is(attr.TokenFactory));

        public static IEnumerable<T> GetBelongings<T>(this Type factoryType)
            => factoryType
                .GetBelongingTypes<T>()
                .Select(t => (T) Activator.CreateInstance(t));

        public static IEnumerable<Type> GetBelongingTypes<T>(this Type factoryType)
            => factoryType
                .Assembly
                .GetTypes()
                .Where(type => type.IsBelongingTo<T>(factoryType));
    }
}