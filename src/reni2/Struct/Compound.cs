using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    sealed class Compound
        : DumpableObject, IContextReference, IChild<ContextBase>
            , ValueCache.IContainer
            , IRootProvider

    {
        static int _nextObjectId;

        [Node]
        internal readonly CompoundSyntax Syntax;

        [Node]
        internal readonly ContextBase Parent;

        readonly int _order;

        [DisableDump]
        internal readonly FunctionCache<int, CompoundView> View;

        internal Compound(CompoundSyntax syntax, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            Syntax = syntax;
            Parent = parent;
            View = new FunctionCache<int, CompoundView>
                (position => new CompoundView(this, position));
        }

        Root IRootProvider.Value => Root;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        public string GetCompoundIdentificationDump() => Syntax.GetCompoundIdentificationDump();

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

        int IContextReference.Order => _order;

        [DisableDump]
        internal TypeBase IndexType => Parent.RootContext.BitType.Number(IndexSize.ToInt());

        [DisableDump]
        internal Root Root => Parent.RootContext;

        [DisableDump]
        internal CompoundView CompoundView => Parent.CompoundView(Syntax);

        Size IndexSize => Syntax.IndexSize;

        [DisableDump]
        internal IEnumerable<ResultCache> CachedResults => Syntax.EndPosition.Select(CachedResult);

        ResultCache CachedResult(int position)
            => Parent
                .CompoundPositionContext(Syntax, position)
                .ResultCache(Syntax.Statements[position]);

        internal Size Size(int? position = null)
        {
            if(Hllw(position))
                return Basics.Size.Zero;
            return ResultsOfStatements(Category.Size, 0, position).Size;
        }

        internal bool Hllw(int? accessPosition = null) => ObtainHllw(accessPosition);

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
                Dump("Statements", Syntax.Statements);
                BreakExecution();

                var statements = ((fromNotPosition ?? EndPosition) - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !Syntax.Statements[position].IsLambda)
                    .Select(position => AccessResult(category, position))
                    .Select(r => r.Align.LocalBlock(category))
                    .ToArray();
                Dump("Statements", statements);
                BreakExecution();
                var result = statements
                    .Aggregate
                    (
                        Parent.RootContext.VoidType.Result(category.Typed),
                        (current, next) => current + next);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        int EndPosition => Syntax.EndPosition;

        internal Result Result(Category category)
        {
            var trace = Syntax.ObjectId.In() && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                var resultsOfStatements = ResultsOfStatements
                    (category - Category.Type, 0, Syntax.EndPosition);

                Dump("resultsOfStatements", resultsOfStatements);
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
                    .ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void)
                    & category;

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
            Tracer.Assert(!Syntax.Statements[position].IsLambda);
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
                Dump(nameof(Syntax.Statements), Syntax.Statements[position]);
                BreakExecution();
                var rawResult = uniqueChildContext.Result
                    (category.Typed, Syntax.Statements[position]);
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

        bool ObtainHllw(int? accessPosition)
        {
            var trace = ObjectId == -10 && accessPosition == 3 && Parent.ObjectId == 4;
            StartMethodDump(trace, accessPosition);
            try
            {
                var subStatementIds = (accessPosition ?? EndPosition).Select().ToArray();
                Dump("subStatementIds", subStatementIds);
                BreakExecution();
                if(subStatementIds.Any(position => InnerHllwStatic(position) == false))
                    return ReturnMethodDump(false);
                var quickNonDataLess = subStatementIds
                    .Where(position => InnerHllwStatic(position) == null)
                    .ToArray();
                Dump("quickNonDataLess", quickNonDataLess);
                BreakExecution();
                if(quickNonDataLess.Length == 0)
                    return ReturnMethodDump(true);
                if(quickNonDataLess.Any
                    (position => InternalInnerHllwStructureElement(position) == false))
                    return ReturnMethodDump(false);
                return ReturnMethodDump(true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        bool InternalInnerHllwStructureElement(int position)
        {
            var uniqueChildContext = Parent
                .CompoundPositionContext(Syntax, position);
            return Syntax
                .Statements[position]
                .HllwStructureElement(uniqueChildContext);
        }

        bool? InnerHllwStatic(int position) => Syntax.Statements[position].Hllw;

        ContextBase IChild<ContextBase>.Parent => Parent;

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
    }
}