using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code.ReplaceVisitor;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalReferenceSequenceVisitor : Base
    {
        private readonly SimpleCache<CodeBase> _codeCache;

        [Node, IsDumpEnabled(true)]
        private readonly List<LocalReference> _data = new List<LocalReference>();

        public LocalReferenceSequenceVisitor() { _codeCache = new SimpleCache<CodeBase>(Convert); }

        private CodeBase Convert()
        {
            return _data
                .Select(localReference => localReference.Code)
                .ToSequence();
        }

        [IsDumpEnabled(false)]
        private CodeBase Code { get { return _codeCache.Value; } }

        private CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return _data
                    .Select(localReference => localReference.AccompayningDestructorCode(ref size))
                    .ToSequence();
            }
        }

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            var newVisitedObject = ReVisit(visitedObject) ?? visitedObject;
            var offset = Find(newVisitedObject);
            _codeCache.Reset();
            return CodeBase.LocalReferenceCode(newVisitedObject.RefAlignParam, offset, "LocalReferenceSequenceVisitor.LocalReference");
        }

        private Size Find(LocalReference localReference)
        {
            var result = Size.Zero;
            var i = 0;
            for(; i < _data.Count && _data[i] != localReference; i++)
                result += _data[i].Code.Size;
            if(i == _data.Count)
                _data.Add(localReference);
            return result + localReference.Code.Size;
        }

        internal CodeBase LocalBlock(CodeBase body, CodeBase copier, RefAlignParam refAlignParam)
        {
            Tracer.Assert(!body.HasArg, body.Dump);
            var trace = body.ObjectId == 736 || body.ObjectId == -440;
            StartMethodDumpWithBreak(trace, body, copier, refAlignParam);
            var newBody = body.Visit(this) ?? body;
            var alignedBody = newBody.Align();
            var resultSize = alignedBody.Size;
            var alignedInternal = Code.Align();
            // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
            var gap = CodeBase.Void();
            DumpWithBreak(trace, "newBody", newBody, "alignedBody", alignedBody, "alignedInternal", alignedInternal);
            if(!copier.IsEmpty && alignedInternal.Size > Size.Zero && alignedInternal.Size < resultSize)
                gap = CodeBase.BitsConst(resultSize - alignedInternal.Size, BitsConst.None());
            var statement = alignedInternal
                .Sequence(gap, alignedBody, DestructorCode)
                .CreateLocalBlockEnd(copier, refAlignParam, resultSize);
            DumpWithBreak(trace, "statement", statement);
            var result = statement.ReplaceArg(CodeBase.TopRef(refAlignParam, "LocalReferenceSequenceVisitor.LocalBlock"));
            return ReturnMethodDump(trace, result);
        }
    }
}