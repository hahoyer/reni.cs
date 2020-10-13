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

        internal readonly Reni.Parser.Syntax FlatSyntax;
        internal new BinaryTree FlatItem=>base.FlatItem;

        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.Parser.Syntax flatSyntax, BinaryTree binary, Syntax parent)
            : base(binary, parent)
        {
            FlatSyntax = flatSyntax?? Parent.FindSyntax(FlatItem);
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
        }

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

        Reni.Parser.Syntax FindSyntax(BinaryTree target)
        {
            var result = FlatSyntax
                .DirectChildren
                .FirstOrDefault(node => node?.Binary == target);
            if(result != null)
                return result;

            Tracer.Assert(FlatSyntax.GetNodesFromLeftToRight().All(node => node?.Binary != target));

            return new FillerSyntax(this, target);
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
                .FirstOrDefault(node => node.Token.Characters.EndPosition <= current);
    }

    class FillerSyntax : Reni.Parser.Syntax.NoChildren
    {
        readonly Syntax Parent;

        public FillerSyntax(Syntax parent, BinaryTree target)
            : base(target)
            => Parent = parent;
    }
}