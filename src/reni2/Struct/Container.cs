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
    sealed class Container : DumpableObject, IContextReference
    {
        static int _nextObjectId;
        readonly ContainerSyntax _syntax;
        readonly ContextBase _parent;
        readonly int _order;

        internal Container(ContainerSyntax syntax, ContextBase parent)
            : base(_nextObjectId++)
        {
            _order = CodeArgs.NextOrder++;
            _syntax = syntax;
            _parent = parent;
        }

        int IContextReference.Order { get { return _order; } }
        Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
        [Node]
        internal ContainerSyntax Syntax { get { return _syntax; } }
        [Node]
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal TypeBase IndexType { get { return _parent.RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }

        [DisableDump]
        internal Root RootContext { get { return Parent.RootContext; } }

        [DisableDump]
        ContainerView ToContainerView { get { return _parent.UniqueStructure(Syntax); } }

        Size IndexSize { get { return Syntax.IndexSize; } }

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
                .UniqueStructure(Syntax, accessPosition)
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
                Dump("Statements", _syntax.Statements);
                var statements = (fromNotPosition - fromPosition)
                    .Select(i => fromPosition + i)
                    .Where(position => !_syntax.Statements[position].IsLambda)
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
            var result = StructureResult(category - Category.Type, 0, Syntax.EndPosition)
                .ReplaceRelative(this, CodeBase.TopRef, CodeArgs.Void);
            if(category.HasType)
                result.Type = ToContainerView.Type;
            return result;
        }

        Result AccessResult(Category category, int position)
        {
            Tracer.Assert(!Syntax.Statements[position].IsLambda);
            return AccessResult(category, position, position);
        }

        Result AccessResult(Category category, int accessPosition, int position)
        {
            var trace = ObjectId.In(-1) && accessPosition >= 0 && position >= 0 && category.HasType;
            StartMethodDump(trace, category, accessPosition, position);
            try
            {
                var uniqueChildContext = Parent
                    .UniqueStructurePositionContext(Syntax, accessPosition);
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
        {
            var trace = ObjectId == -10 && accessPosition == 1 && position == 0;
            StartMethodDump(trace, accessPosition, position);
            try
            {
                Dump("Statements[position]", Syntax.Statements[position]);
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
                .UniqueStructurePositionContext(Syntax, position);
            return Syntax
                .Statements[position]
                .HllwStructureElement(uniqueChildContext);
        }

        bool? InnerHllwStatic(int position) { return Syntax.Statements[position].Hllw; }

        internal string[] DataIndexList()
        {
            return Syntax
                .Statements
                .Length
                .Select()
                .Where(i => !InternalInnerHllwStructureElement(i))
                .Select(i => i.ToString() + "=" + AccessResult(Category.Size, i).Size.ToString())
                .ToArray();
        }
    }
}