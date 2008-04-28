using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Syntax
{
    /// <summary>
    /// Statement inside of a struct, context free version
    /// </summary>
    internal sealed class Statement : SyntaxBase
    {
        private readonly List<MemberElem> _chain;

        private Statement(List<MemberElem> chain, MemberElem tail)
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

        internal override Result VirtVisit(ContextBase context, Category category)
        {
            var trace = ObjectId == 21 && context.ObjectId == 0;
            StartMethodDumpWithBreak(trace, context, category);
            if(Chain.Count == 0)
                NotImplementedMethod(context, category);

            var internalCategory = category | Category.Type;
            if(category.HasCode)
                internalCategory |= Category.Refs;
            var intermediateResult = TypeBase.CreateVoidResult(internalCategory);
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
                Tracer.ConditionalBreak(trace, result .Dump());
                if (internalCategory.HasRefs)
                    foreach(var referencedContext in result.Refs.Data)
                        if(referencedContext.IsChildOf(context))
                        {
                            var replaceContextCode = intermediateResult.Type.CreateRefCodeForContext(referencedContext);
                            Tracer.Assert(replaceContextCode != null);
                            result = result.ReplaceRelativeContextRef(referencedContext, replaceContextCode);
                        }
            }

            if(result.IsPending)
                return result;
            Tracer.Assert(result != null);
            var dereferencedResult = result.Type.Dereference(result).PostProcess(context);
            var statementResult = dereferencedResult.CreateStatement(category, intermediateResult);
            return ReturnMethodDumpWithBreak(trace, statementResult);
        }

        internal override SyntaxBase CreateDefinableSyntax(DefineableToken defineableToken, SyntaxBase right)
        {
            return new Statement(_chain, new MemberElem(defineableToken, right));
        }

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