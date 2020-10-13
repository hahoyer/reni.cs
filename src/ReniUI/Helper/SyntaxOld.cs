using System.Collections.Generic;
using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class SyntaxOld : SyntaxWithParent<SyntaxOld>
    {
        class CacheContainer
        {
            public FunctionCache<int, SyntaxOld> ExtendedLocateByPosition;
            public FunctionCache<int, SyntaxOld> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal SyntaxOld(Syntax flatItem, SyntaxOld parent = null)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, SyntaxOld>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, SyntaxOld>(ExtendedLocateByPositionForCache);
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
        internal IEnumerable<SyntaxOld> ParentChainIncludingThis
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
        new Syntax FlatItem => base.FlatItem;

        protected override string GetNodeDump() => $"{GetType().PrettyName()}({FlatItem.GetType().PrettyName()})";

        public SyntaxOld LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override SyntaxOld Create(Syntax flatItem)
            => new SyntaxOld(flatItem, this);

        internal SyntaxOld Locate(SourcePart part)
        {
            NotImplementedMethod(part);
            return default;
        }

        SyntaxOld CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        SyntaxOld LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        SyntaxOld LocateByPositionForCache(int current)
        {
            NotImplementedMethod(current);
            return default;
        }

        static bool Touches(int targetPosition, SyntaxOld target)
        {
            var token = target.FlatItem.Token;
            if(token == null)
                return false;
            var sourcePart = token.SourcePart();
            if(sourcePart.EndPosition < targetPosition)
                return false;
            Tracer.Assert(sourcePart.Position <= targetPosition);
            return true;
        }

        SyntaxOld ExtendedLocateByPositionForCache(int current)
        {
            NotImplementedMethod(current);
            return default;
        }


        SyntaxOld LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        SyntaxOld ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}