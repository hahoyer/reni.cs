using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct
{
    sealed class Compound
        : DumpableObject, IContextReference, IChild<ContextBase>, ValueCache.IContainer, IRootProvider

    {
        static int _nextObjectId;

        readonly int _order;

        [Node]
        internal readonly ContextBase Parent;

        [Node]
        internal readonly CompoundSyntax Syntax;

        [DisableDump]
        internal readonly FunctionCache<int, CompoundView> View;

        internal Compound(CompoundSyntax syntax, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = Closures.NextOrder++;
            Syntax = syntax;
            Parent = parent;
            View = new FunctionCache<int, CompoundView>
                (position => new CompoundView(this, position));
            StopByObjectIds();
        }

        ContextBase IChild<ContextBase>.Parent => Parent;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        int IContextReference.Order => _order;

        Root IRootProvider.Value => Root;

        [DisableDump]
        internal TypeBase IndexType => Parent.RootContext.BitType.Number(IndexSize.ToInt());

        [DisableDump]
        internal Root Root => Parent.RootContext;

        [DisableDump]
        internal CompoundView CompoundView => Parent.CompoundView(Syntax);

        Size IndexSize => Syntax.IndexSize;

        [DisableDump]
        internal IEnumerable<ResultCache> CachedResults => Syntax.EndPosition.Select(CachedResult);

        int EndPosition => Syntax.EndPosition;
        public bool HasIssue => Issues.Any();
        public Issue[] Issues => GetIssues();

        public string GetCompoundIdentificationDump() => Syntax.GetCompoundIdentificationDump();

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

        ResultCache CachedResult(int position)
            => Parent
                .CompoundPositionContext(Syntax, position)
                .ResultCache(Syntax.PureStatements[position]);

        internal Size Size(int? position = null)
        {
            if(IsHollow(position))
                return Basics.Size.Zero;
            return ResultsOfStatements(Category.Size, fromPosition: 0, fromNotPosition: position).Size;
        }

        internal bool IsHollow(int? accessPosition = null) => ObtainIsHollow(accessPosition);

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
            => ResultsOfStatements(Category.Size, fieldPosition + 1, accessPosition).Size;

        Result ResultsOfStatements(Category category, int fromPosition, int? fromNotPosition)
        {
            if(category.IsNone)
                return new Result();
            var trace = Syntax.ObjectId.In() && category.HasCode;
            StartMethodDump(trace, category, fromPosition, fromNotPosition);
            try
            {
                Dump(nameof(Syntax.PureStatements), Syntax.PureStatements);
                BreakExecution();

                var positions = ((fromNotPosition ?? EndPosition) - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !Syntax.PureStatements[position].IsLambda)
                    .ToArray();

                var rawResults = positions
                    .Select(position => AccessResult(category, position))
                    .ToArray();

                var results = rawResults
                    .Select(r => r.Align.LocalBlock(category))
                    .ToArray();
                Dump(nameof(results), results);
                BreakExecution();
                var result = results
                    .Aggregate
                    (
                        Parent.RootContext.VoidType.Result(category.WithType),
                        (current, next) => current.Sequence(next,Syntax.Target.SourcePart)
                    );
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Result(Category category)
        {
            var trace = Syntax.ObjectId.In() && category.HasType;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var resultsOfStatements = ResultsOfStatements
                    (category - Category.Type, fromPosition: 0, fromNotPosition: Syntax.EndPosition);

                Dump(name: "resultsOfStatements", value: resultsOfStatements);
                BreakExecution();

                var aggregate = Syntax
                    .EndPosition
                    .Select()
                    .Aggregate(resultsOfStatements, Combine);

                Dump(nameof(aggregate), aggregate);
                BreakExecution();

                var resultWithCleanup = aggregate.ArrangeCleanupCode();
                Dump(nameof(resultWithCleanup), resultWithCleanup);
                BreakExecution();

                var result = resultWithCleanup
                                 .ReplaceRelative(this, CodeBase.TopRef, Closures.Void) &
                             category;

                if(result.HasIssue)
                    return ReturnMethodDump(result);

                if(category.HasType)
                    result.Type = CompoundView.Type;

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result Combine(Result result, int position)
            => Parent.CompoundView(Syntax, position).ReplaceObjectPointerByContext(result);

        Result AccessResult(Category category, int position)
        {
            Tracer.Assert(!Syntax.PureStatements[position].IsLambda);
            return AccessResult(category, position, position);
        }

        Result AccessResult(Category category, int accessPosition, int position)
        {
            var trace = Syntax.ObjectId.In()
                && accessPosition == 0
                && position == 0
                && category.HasCode
                ;
            StartMethodDump(trace, category, accessPosition, position);
            try
            {
                var uniqueChildContext = Parent.CompoundPositionContext(Syntax, accessPosition);
                Dump(nameof(Syntax.PureStatements), Syntax.PureStatements[position]);
                BreakExecution();
                var rawResult = uniqueChildContext.Result
                    (category.WithType, Syntax.PureStatements[position]);
                Dump(nameof(rawResult), rawResult);
                BreakExecution();
                var unFunction = rawResult.SmartUn<FunctionType>();
                Dump(nameof(unFunction), unFunction);
                BreakExecution();
                var result = unFunction.AutomaticDereferenceResult;
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal TypeBase AccessType(int accessPosition, int position)
            => AccessResult(Category.Type, accessPosition, position).Type;

        bool ObtainIsHollow(int? accessPosition)
        {
            var trace = ObjectId == -10 && accessPosition == 3 && Parent.ObjectId == 4;
            StartMethodDump(trace, accessPosition);
            try
            {
                var subStatementIds = (accessPosition ?? EndPosition).Select().ToArray();
                Dump(name: "subStatementIds", value: subStatementIds);
                BreakExecution();
                if(subStatementIds.Any(position => InnerIsHollowStatic(position) == false))
                    return ReturnMethodDump(rv: false);
                var quickNonDataLess = subStatementIds
                    .Where(position => InnerIsHollowStatic(position) == null)
                    .ToArray();
                Dump(name: "quickNonDataLess", value: quickNonDataLess);
                BreakExecution();
                if(quickNonDataLess.Length == 0)
                    return ReturnMethodDump(rv: true);
                if(quickNonDataLess.Any
                    (position => InternalInnerIsHollowStructureElement(position) == false))
                    return ReturnMethodDump(rv: false);
                return ReturnMethodDump(rv: true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        bool InternalInnerIsHollowStructureElement(int position)
        {
            var uniqueChildContext = Parent
                .CompoundPositionContext(Syntax, position);
            return Syntax
                .PureStatements[position]
                .IsHollowStructureElement(uniqueChildContext);
        }

        bool? InnerIsHollowStatic(int position) => Syntax.PureStatements[position].IsHollow;

        internal Result Cleanup(Category category)
        {
            var uniqueChildContext = Parent.CompoundPositionContext(Syntax);
            var cleanup = Syntax.Cleanup(uniqueChildContext, category);
            var aggregate = EndPosition
                .Select()
                .Reverse()
                .Select(index => GetCleanup(category, index))
                .Aggregate();
            return cleanup + aggregate;
        }

        Result GetCleanup(Category category, int index)
        {
            var result = AccessType(EndPosition, index).Cleanup(category);
            Tracer.Assert(result.CompleteCategory == category);
            return result;
        }

        internal Issue[] GetIssues(int? viewPosition = null)
            => ResultsOfStatements(Category.Type, fromPosition: 0, fromNotPosition: viewPosition)
                .Issues;
    }
}