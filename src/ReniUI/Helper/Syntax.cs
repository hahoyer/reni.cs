using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.CompilationView;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> ExtendedLocateByPosition;
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.Parser.Syntax flatItem)
            : this(flatItem, null) { }

        Syntax(Reni.Parser.Syntax flatItem, Syntax parent)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, Syntax>(ExtendedLocateByPositionForCache);
        }

        BinaryTree Binary;


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

        protected override Syntax Create(Reni.Parser.Syntax target, Syntax parent) => new Syntax(target, parent);

        internal Syntax Locate(SourcePart part)
        {
            NotImplementedMethod(part);
            return default;
        }

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Syntax LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        Syntax LocateByPositionForCache(int current)
        {
            var s = SourcePart;
            //var e = this.GetNodesFromLeftToRight().Select(target=>target.Binary.GetSource()?.EndPosition).ToArray();
            var result = this
                .GetNodesFromLeftToRight()
                .FirstOrDefault(node=>Touches(current, node));
            if(result == null)
            {
                NotImplementedMethod(current);
                return default;

            }
            
            Tracer.Assert(result.FlatItem.GetSource().Position <= current);
            return result;
        }

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