using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
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
        internal readonly CompoundSyntax Syntax;
        [Node]
        internal readonly ContextBase Parent;

        readonly int _order;

        internal Compound(CompoundSyntax syntax, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            Syntax = syntax;
            Parent = parent;
        }

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

        internal Result ContextReferenceViaStructReference(int position, Result result)
            => result.ReplaceAbsolute(this, () => ContextReferenceViaStructReferenceCode(position), CodeArgs.Arg);

        internal Size ContextReferenceOffsetFromAccessPoint(int position) => ResultsOfStatements(Category.Size, 0, position).Size;

        CodeBase ContextReferenceViaStructReferenceCode(int accessPosition) => Parent
            .CompoundView(Syntax, accessPosition)
            .PointerKind.ArgCode
            .ReferencePlus(ContextReferenceOffsetFromAccessPoint(accessPosition));

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
            => ResultsOfStatements(Category.Size, fieldPosition + 1, accessPosition).Size;

        Result ResultsOfStatements(Category category, int fromPosition, int fromNotPosition)
        {
            if(category.IsNone)
                return new Result();
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, fromPosition, fromNotPosition);
            try
            {
                Dump("Statements", Syntax.Statements);
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
                    .Aggregate(Parent.RootContext.VoidResult(category), (current, next) => current + next);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Result(Category category)
        {
            var resultsOfStatements = ResultsOfStatements(category - Category.Type, 0, Syntax.EndPosition);
            var result = resultsOfStatements.ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void);
            if(category.HasType)
                result.Type = ToCompoundView.Type;
            return result;
        }

        Result AccessResult(Category category, int position)
        {
            Tracer.Assert(!Syntax.Statements[position].IsLambda);
            return AccessResult(category, position, position);
        }

        Result AccessResult(Category category, int accessPosition, int position)
        {
            var trace = ObjectId.In(-1) && accessPosition >= 0 && position == 1 && category.HasCode;
            StartMethodDump(trace, category, accessPosition, position);
            try
            {
                var uniqueChildContext = Parent
                    .StructurePositionContext(Syntax, accessPosition);
                Dump("Statements[position]", Syntax.Statements[position]);
                BreakExecution();
                var result1 = Syntax.Statements[position]
                    .Result(uniqueChildContext, category.Typed);
                Dump("result1", result1);
                BreakExecution();
                var result = result1
                    .SmartUn<FunctionType>();
                Dump("result", result);
                return ReturnMethodDump(result.AutomaticDereferenceResult);
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
                if(quickNonDataLess.Any(position => InternalInnerHllwStructureElement(position) == false))
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
                .StructurePositionContext(Syntax, position);
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