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
        [Node, EnableDump]
        private readonly DictionaryEx<LocalReference, int> _localReferences;

        private static int _nextObjectId;

        public LocalReferenceSequenceVisitor()
            : base(_nextObjectId++)
        {
            _localReferences = new DictionaryEx<LocalReference, int>(-1, ObtainHolderIndex);
            _codeCache = new SimpleCache<CodeBase>(Convert);
        }

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
                    .Select((localReference, i) => localReference.AccompayningDestructorCode(ref size,HolderName(i)))
                    .ToSequence();
            }
        }

        private string HolderNamePattern { get { return "h_" + ObjectId + "_{0}"; } }

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            StartMethodDump(ObjectId == -10, visitedObject);
            try
            {
                BreakExecution();
                var holderIndex = _localReferences.Find(visitedObject);
                Dump("holderIndex", holderIndex);
                _codeCache.Reset();
                return ReturnMethodDump(CodeBase.LocalVariableReference(visitedObject.RefAlignParam, HolderName(holderIndex)),true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        private string HolderName(int holderIndex) { return string.Format(HolderNamePattern, holderIndex); }

        private int ObtainHolderIndex(LocalReference visitedObject)
        {
            _data.Add(ReVisit(visitedObject) ?? visitedObject);
            return _data.Count-1;
        }

        internal CodeBase LocalBlock(CodeBase body, CodeBase copier, RefAlignParam refAlignParam)
        {
            Tracer.Assert(!body.HasArg, body.Dump);
            var trace = ObjectId == -6;
            StartMethodDump(trace, body, copier, refAlignParam);
            try
            {
                BreakExecution();
                var newBody = body.Visit(this) ?? body;
                var alignedBody = newBody.Align();
                var resultSize = alignedBody.Size;
                var alignedInternal = Code.Align();
                // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
                var gap = CodeBase.Void();
                Dump("newBody", newBody);
                Dump("aligned Body", alignedBody);
                Dump("alignedInternal", alignedInternal);
                BreakExecution();
                if (!copier.IsEmpty && alignedInternal.Size > Size.Zero && alignedInternal.Size < resultSize)
                    gap = CodeBase.BitsConst(resultSize - alignedInternal.Size, BitsConst.None());
                var statement = alignedInternal
                    .Sequence(gap, alignedBody, DestructorCode)
                    .LocalBlockEnd(copier, refAlignParam, resultSize, HolderNamePattern);
                Dump("statement", statement);
                BreakExecution();
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