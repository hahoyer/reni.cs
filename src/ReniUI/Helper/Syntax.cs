using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> ExtendedLocateByPosition;
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        [DisableDump]
        internal BinaryTree Binary;

        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.Parser.Syntax flatItem, Syntax parent = null)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, Syntax>(ExtendedLocateByPositionForCache);
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Issue[] Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string[] DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        internal IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        [DisableDump]
        bool IsFunctionLevel => TokenClass is Function;

        [EnableDump]
        new Reni.Parser.Syntax FlatItem => base.FlatItem;

        protected override string GetNodeDump() => $"{GetType().PrettyName()}({FlatItem.GetType().PrettyName()})";

        public Syntax LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override Syntax Create(Reni.Parser.Syntax flatItem)
            => new Syntax(flatItem, this);

        internal Syntax Locate(SourcePart part)
        {
            NotImplementedMethod(part);
            return default;
        }

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Syntax LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        Syntax LocateByPositionForCache(int current) 
            => Binary
                .LocateByPosition(current)
                .ParentChainIncludingThis
                .First(node => node.Syntax != null)
                .Syntax;

        static bool Touches(int targetPosition, Syntax target)
        {
            var token = target.FlatItem.Binary?.Token;
            if(token == null)
                return false;
            var sourcePart = token.SourcePart();
            if(sourcePart.EndPosition < targetPosition)
                return false;
            Tracer.Assert(sourcePart.Position <= targetPosition);
            return true;
        }

        Syntax ExtendedLocateByPositionForCache(int current)
        {
            NotImplementedMethod(current);
            return default;
        }


        Syntax LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        Syntax ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}