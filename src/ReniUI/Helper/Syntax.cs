using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class Syntax : BinaryTreeSyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly Reni.Parser.Syntax FlatSyntaxRoot;

        internal Syntax(Reni.Parser.Syntax flatSyntax, BinaryTree binary, Syntax parent)
            : base(binary, parent)
        {
            FlatSyntaxRoot = flatSyntax;
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
        }

        [DisableDump]
        internal Reni.Parser.Syntax FlatSyntax => this.CachedValue(() => FlatSyntaxRoot ?? GetFlatSyntax());

        internal new BinaryTree FlatItem => base.FlatItem;

        [DisableDump]
        public Issue[] Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public string[] DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        internal IEnumerable<Syntax> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        IEnumerable<Syntax> GetParserLevelBelongings()
        {
            NotImplementedMethod();
            return default;
        }

        Reni.Parser.Syntax GetFlatSyntax()
        {
            NotImplementedMethod();
            return default;
        }

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(null, flatItem, this);


        internal Syntax LocateByPosition(int offset) => Cache.LocateByPosition[offset];

        public Syntax Locate(SourcePart span)
        {
            NotImplementedMethod(span);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);

        Syntax LocateByPositionForCache(int current)
            => this
                .GetNodesFromLeftToRight()
                .FirstOrDefault(node => node.Token.Characters.EndPosition > current);
    }

    class ProxySyntax : Reni.Parser.Syntax.NoChildren
    {
        [DisableDump]
        internal readonly Syntax Client;

        public ProxySyntax(Syntax client, BinaryTree target)
            : base(target)
            => Client = client;
    }
}