using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalReferenceSequenceVisitor : Base
    {
        private readonly SimpleCache<CodeBase> _codeCache;

        [Node, EnableDump]
        private readonly List<LocalReference> _data = new List<LocalReference>();

        private static int _nextObjectId;

        public LocalReferenceSequenceVisitor()
            : base(_nextObjectId++) { _codeCache = new SimpleCache<CodeBase>(Convert); }

        private CodeBase Convert()
        {
            return _data
                .Select(localReference => localReference.Code)
                .ToLocalVariables(HolderNamePattern);
        }

        [DisableDump]
        private CodeBase Code { get { return _codeCache.Value; } }

        private CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return _data
                    .Select((localReference, i) => localReference.AccompayningDestructorCode(ref size, string.Format(HolderNamePattern, i)))
                    .ToSequence();
            }
        }

        private string HolderNamePattern { get { return "h_" + ObjectId + "_{0}"; } }

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            var newVisitedObject = ReVisit(visitedObject) ?? visitedObject;
            var i = Find(newVisitedObject);
            _codeCache.Reset();
            return CodeBase.LocalVariableReference(newVisitedObject.RefAlignParam, string.Format(HolderNamePattern, i));
        }

        private int Find(LocalReference localReference)
        {
            for(var i = 0; i < _data.Count; i++)
            {
                if(_data[i] == localReference)
                    return i;
            }
            _data.Add(localReference);
            return _data.Count - 1;
        }

        internal CodeBase LocalBlock(CodeBase body, CodeBase copier, RefAlignParam refAlignParam)
        {
            Tracer.Assert(!body.HasArg, body.Dump);
            var trace = ObjectId == -2;
            BreakNext(); StartMethodDump(trace, body);
            try
            {
                var newBody = body.Visit(this) ?? body;
                var alignedBody = newBody.Align();
                var resultSize = alignedBody.Size;
                var alignedInternal = Code.Align();
                // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
                var gap = CodeBase.Void();
                Dump("newBody", newBody);
                Dump("aligned Body", alignedBody);
                BreakNext(); Dump("alignedInternal", alignedInternal);
                if (!copier.IsEmpty && alignedInternal.Size > Size.Zero && alignedInternal.Size < resultSize)
                    gap = CodeBase.BitsConst(resultSize - alignedInternal.Size, BitsConst.None());
                var statement = alignedInternal
                    .Sequence(gap, alignedBody, DestructorCode)
                    .LocalBlockEnd(copier, refAlignParam, resultSize, HolderNamePattern);
                BreakNext(); Dump("statement", statement);
                var result = statement;
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}