using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    sealed class Compound : DumpableObject, IContextReference
    {
        static int _nextObjectId;

        [Node]
        [DisableDump]
        internal readonly CompoundSyntax Syntax;

        [Node]
        [DisableDump]
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
            View = new FunctionCache<int, CompoundView>(position=> new CompoundView(this, position));
        }

        public string GetCompoundIdentificationDump() => Syntax.GetCompoundIdentificationDump();

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

        int IContextReference.Order => _order;
        Size IContextReference.Size => Root.DefaultRefAlignParam.RefSize;

        [DisableDump]
        internal TypeBase IndexType => Parent.RootContext.BitType.Number(IndexSize.ToInt());

        [DisableDump]
        internal Root RootContext => Parent.RootContext;

        [DisableDump]
        CompoundView ToCompoundView => Parent.CompoundView(Syntax);

        Size IndexSize => Syntax.IndexSize;

        internal Size Size(int position)
        {
            if(Hllw(position))
                return Basics.Size.Zero;
            return ResultsOfStatements(Category.Size, 0, position).Size;
        }

        internal bool Hllw(int accessPosition) => ObtainHllw(accessPosition);

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
            => ResultsOfStatements(Category.Size, fieldPosition + 1, accessPosition).Size;

        Result ResultsOfStatements(Category category, int fromPosition, int fromNotPosition)
        {
            if(category.IsNone)
                return new Result();
            var trace = Syntax.ObjectId == -14 && category.HasCode;
            StartMethodDump(trace, category, fromPosition, fromNotPosition);
            try
            {
                Dump("Statements", Syntax.Statements);
                BreakExecution();

                var statements = (fromNotPosition - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !Syntax.Statements[position].IsLambda)
                    .Select(position => AccessResult(category, position))
                    .Select(r => r.Align)
                    .ToArray();
                Dump("Statements", statements);
                BreakExecution();
                var results = statements
                    .Select(r => r.LocalBlock(category))
                    .ToArray();
                Dump("results", results);
                BreakExecution();
                var result = results
                    .Aggregate
                    (
                        Parent.RootContext.VoidType.Result(category),
                        (current, next) => current + next);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Result(Category category)
        {
            var trace = Syntax.ObjectId == -14 && (category.HasCode||category.HasExts);
            StartMethodDump(trace, category);
            try
            {
                var resultsOfStatements = ResultsOfStatements
                    (category - Category.Type, 0, Syntax.EndPosition);

                Dump("resultsOfStatements", resultsOfStatements);
                BreakExecution();

                var result = Syntax
                    .EndPosition
                    .Select()
                    .Aggregate(resultsOfStatements, Combine)
                    .ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void)
                    ;
                if(category.HasType)
                    result.Type = ToCompoundView.Type;
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
                && accessPosition == 2
                && position == 2
                && category.HasCode;
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

        bool ObtainHllw(int accessPosition)
        {
            var trace = ObjectId == -10 && accessPosition == 3 && Parent.ObjectId == 4;
            StartMethodDump(trace, accessPosition);
            try
            {
                var subStatementIds = accessPosition.Select().ToArray();
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

        internal string[] DataIndexList()
            => Syntax
                .Statements
                .Length
                .Select()
                .Where(i => !InternalInnerHllwStructureElement(i))
                .Select(i => i.ToString() + "=" + AccessResult(Category.Size, i).Size.ToString())
                .ToArray();
    }
}