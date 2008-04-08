using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.Syntax
{
    /// <summary>
    /// Statement inside of a struct, context free version
    /// </summary>
    internal sealed class Statement : Base
    {
        private readonly List<MemberElem> _chain;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="chain">The chain.</param>
        /// <param name="tail">The tail.</param>
        private Statement(List<MemberElem> chain, MemberElem tail)
        {
            _chain = new List<MemberElem>();
            _chain.AddRange(chain);
            _chain.Add(tail);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="head">The head.</param>
        /// <param name="tail">The tail.</param>
        public Statement(MemberElem head, MemberElem tail)
        {
            _chain = new List<MemberElem>();
            _chain.Add(head);
            _chain.Add(tail);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="head">The head.</param>
        public Statement(MemberElem head)
        {
            _chain = new List<MemberElem>();
            _chain.Add(head);
        }

        /// <summary>
        /// 
        /// </summary>
        [DebuggerHidden]
        public List<MemberElem> Chain { get { return _chain; } }

        /// <summary>
        /// Visitor function 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        internal override Result VirtVisit(Context.Base context, Category category)
        {
            var trace = ObjectId == 1082 && context.ObjectId == 21;
            StartMethodDumpWithBreak(trace, context, category);
            if(Chain.Count == 0)
                NotImplementedMethod(context, category);

            var internalCategory = category | Category.Type;
            if(category.HasCode)
                internalCategory |= Category.Refs;
            var intermediateResult = Type.Base.CreateVoidResult(internalCategory);
            var result = context.VisitFirstChainElement(internalCategory, Chain[0]);
            for(var i = 1; i < Chain.Count; i++)
            {
                var newResult = result.PostProcess(context);

                Tracer.ConditionalBreak(trace, newResult.Dump());
                if(!newResult.Type.IsRef)
                    intermediateResult = intermediateResult.SafeList(newResult, internalCategory);
                if(newResult.IsPending)
                    return newResult;

                result = context.VisitNextChainElement(internalCategory, Chain[i], newResult);
                if(internalCategory.HasRefs)
                    foreach(var referencedContext in result.Refs.Data)
                    {
                        if(referencedContext.IsChildOf(context))
                        {
                            var replaceContextCode = intermediateResult.Type.CreateRefCodeForContext(referencedContext);
                            Tracer.Assert(replaceContextCode != null);
                            result = result.ReplaceRelativeContextRef(referencedContext, replaceContextCode);
                        }
                    }
            }

            if(result.IsPending)
                return result;
            Tracer.Assert(result != null);
            var dereferencedResult = result.Type.Dereference(result).PostProcess(context);
            var statementResult = dereferencedResult.CreateStatement(category, intermediateResult);
            return ReturnMethodDumpWithBreak(trace, statementResult);
        }

        /// <summary>
        /// Creates the definable syntax.
        /// </summary>
        /// <param name="defineableToken">The defineable token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 01.04.2007 23:06 on SAPHIRE by HH
        internal override Base CreateDefinableSyntax(DefineableToken defineableToken, Base right)
        {
            return new Statement(_chain, new MemberElem(defineableToken, right));
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        /// created 07.05.2007 22:10 on HAHOYER-DELL by hh
        internal override string DumpShort()
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