using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Syntax
{
    /// <summary>
    /// Statement inside of a struct, context free version
    /// </summary>
    internal sealed class Statement : ReniObject
    {
        private readonly List<MemberElem> _chain;

        private Statement(IEnumerable<MemberElem> chain, MemberElem tail)
        {
            _chain = new List<MemberElem>();
            _chain.AddRange(chain);
            _chain.Add(tail);
        }

        public Statement(MemberElem head, MemberElem tail)
        {
            _chain = new List<MemberElem> {head, tail};
        }

        public Statement(MemberElem head)
        {
            _chain = new List<MemberElem> {head};
        }

        [DebuggerHidden]
        public List<MemberElem> Chain { get { return _chain; } }

        internal class VisitStrategy : ReniObject
        {
            private readonly Category _category;
            private readonly ContextBase _context;
            private Result _intermediateResult;
            private Result _result;

            public VisitStrategy(ContextBase contextBase, Category category)
            {
                _context = contextBase;
                _category = category;
                IsComplete = false;
            }

            private bool IsComplete { get; set; }
            private bool IsPending { get { return _result != null && _result.IsPending; } }
            private Category InternalCategory
            {
                get
                {
                    if(_category.HasCode)
                        return _category | Category.Type | Category.Refs;
                    return _category | Category.Type;
                }
            }

            public void Execute(MemberElem memberElem, bool isLast)
            {
                if(IsComplete)
                    return;
                if(IsPending)
                    return;

                _result = _context.VisitChainElement(InternalCategory, memberElem, _result);
                if(IsPending)
                    return;

                if(isLast)
                    _result = _result.Type.Dereference(_result);
                if(IsPending)
                    return;

                _result = _result.PostProcess(_context.RefAlignParam);
                if(IsPending)
                    return;

                if(!isLast)
                    UpdateIntermediateResult();

                ReplaceContextRefs();
            }

            private void UpdateIntermediateResult()
            {
                if(_result.Type.IsRef(_context.RefAlignParam))
                    return;
                _intermediateResult = _intermediateResult == null ? _result : _intermediateResult.SafeList(_result, InternalCategory);
            }

            private void ReplaceContextRefs()
            {
                if(IsPending)
                    return;
                if(!InternalCategory.HasRefs)
                    return;
                if(! (_context is Reni.Struct.Context))
                    return;

                var referencedContexts = _result.Refs.Data;
                foreach(var referencedContext in referencedContexts)
                    if(_intermediateResult != null && referencedContext.IsChildOf(_context))
                    {
                        var replaceContextCode = _intermediateResult.Type.CreateRefCodeForContext(referencedContext);
                        Tracer.Assert(replaceContextCode != null);
                        _result = _result.ReplaceRelativeContextRef(referencedContext, replaceContextCode);
                    }

                //foreach(var referencedContext in _result.Refs.Data)
                //    Tracer.Assert(!_context.IsStructParentOf(referencedContext));
            }

            public Result FinalizeResult()
            {
                if(IsComplete || _result == null)
                    NotImplementedMethod();
                IsComplete = true;
                if(IsPending)
                    return _result;
                return _result.CreateStatement(_category, _intermediateResult);
            }
        }

        public Result VirtVisit(ContextBase context, Category category)
        {
            var visitStrategy = new VisitStrategy(context, category);
            for(var i = 0; i < Chain.Count; i++)
                visitStrategy.Execute(Chain[i], i == Chain.Count - 1);
            return visitStrategy.FinalizeResult();
        }

        internal string DumpShort()
        {
            var result = "";
            for(var i = 0; i < _chain.Count; i++)
            {
                if(i > 0)
                    result += " ";
                result += _chain[i].DumpShort();
            }
            return result;
        }
    }
}