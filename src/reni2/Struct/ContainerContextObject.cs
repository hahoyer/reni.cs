using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    sealed class ContainerContextObject : DumpableObject, IContextReference
    {
        static int _nextObjectId;
        readonly Container _container;
        readonly ContextBase _parent;
        readonly int _order;

        internal ContainerContextObject(Container container, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _container = container;
            _parent = parent;
        }

        int IContextReference.Order { get { return _order; } }
        Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
        [Node]
        internal Container Container { get { return _container; } }
        [Node]
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal TypeBase IndexType { get { return _parent.RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }

        [DisableDump]
        internal Root RootContext { get { return Parent.RootContext; } }

        [DisableDump]
        Structure ToStructure { get { return _parent.UniqueStructure(Container); } }

        Size IndexSize { get { return Container.IndexSize; } }

        internal Size StructureSize(int position)
        {
            if(StructureHllw(position))
                return Size.Zero;
            return StructureResult(Category.Size, 0, position).Size;
        }

        internal bool StructureHllw(int accessPosition) { return ObtainHllw(accessPosition); }

        internal Result ContextReferenceViaStructReference(int position, Result result)
        {
            return result.ReplaceAbsolute(this, () => ContextReferenceViaStructReferenceCode(position), CodeArgs.Arg);
        }

        internal Size ContextReferenceOffsetFromAccessPoint(int position)
        {
            return StructureResult(Category.Size, 0, position).Size;
        }

        CodeBase ContextReferenceViaStructReferenceCode(int accessPosition)
        {
            return Parent
                .UniqueStructure(Container, accessPosition)
                .PointerKind.ArgCode
                .ReferencePlus(ContextReferenceOffsetFromAccessPoint(accessPosition));
        }

        internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
        {
            return StructureResult(Category.Size, fieldPosition + 1, accessPosition).Size;
        }

        Result StructureResult(Category category, int fromPosition, int fromNotPosition)
        {
            if(category.IsNone)
                return new Result();
            var trace = ObjectId == 0 && category.HasCode;
            StartMethodDump(trace, category, fromPosition, fromNotPosition);
            try
            {
                Dump("Statements", _container.Statements);
                var statements = (fromNotPosition - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !_container.Statements[position].IsLambda)
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
            var result = StructureResult(category - Category.Type, 0, Container.EndPosition)
                .ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void);
            if(category.HasType)
                result.Type = ToStructure.Type;
            return result;
        }

        Result AccessResult(Category category, int position)
        {
            Tracer.Assert(!Container.Statements[position].IsLambda);
            return AccessResult(category, position, position);
        }

        Result AccessResult(Category category, int accessPosition, int position)
        {
            var trace = ObjectId.In(-1) && accessPosition >= 0 && position >= 0 && category.HasType;
            StartMethodDump(trace, category, accessPosition, position);
            try
            {
                var uniqueChildContext = Parent
                    .UniqueStructurePositionContext(Container, accessPosition);
                Dump("Statements[position]", Container.Statements[position]);
                BreakExecution();
                var result1 = Container.Statements[position]
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
        {
            var trace = ObjectId == -10 && accessPosition == 1 && position == 0;
            StartMethodDump(trace, accessPosition, position);
            try
            {
                Dump("Statements[position]", Container.Statements[position]);
                BreakExecution();
                var result = AccessResult(Category.Type, accessPosition, position).Type;
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

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
                .UniqueStructurePositionContext(Container, position);
            return Container
                .Statements[position]
                .HllwStructureElement(uniqueChildContext);
        }

        bool? InnerHllwStatic(int position) { return Container.Statements[position].Hllw; }

        internal string[] DataIndexList()
        {
            return Container
                .Statements
                .Length
                .Select()
                .Where(i => !InternalInnerHllwStructureElement(i))
                .Select(i => i.ToString() + "=" + AccessResult(Category.Size, i).Size.ToString())
                .ToArray();
        }
    }
}