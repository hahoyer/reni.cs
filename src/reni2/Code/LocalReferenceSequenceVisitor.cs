using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;

namespace Reni.Code
{
    sealed class LocalReferenceSequenceVisitor : Base
    {
        readonly ValueCache<CodeBase> _codeCache;

        [Node]
        [EnableDump]
        readonly List<LocalReference> _data = new List<LocalReference>();
        [Node]
        [EnableDump]
        readonly FunctionCache<LocalReference, int> _localReferences;

        static int _nextObjectId;

        public LocalReferenceSequenceVisitor()
            : base(_nextObjectId++)
        {
            _localReferences = new FunctionCache<LocalReference, int>(-1, ObtainHolderIndex);
            _codeCache = new ValueCache<CodeBase>(Convert);
        }

        CodeBase Convert()
        {
            return ToLocalVariables(_data.Select(localReference => localReference.Code));
        }

        [DisableDump]
        CodeBase Code => _codeCache.Value;

        CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return _data
                    .Select((localReference, i) => localReference.AccompayningDestructorCode(ref size, new Holder(i,ObjectId)))
                    .ToSequence();
            }
        }

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            StartMethodDump(ObjectId == -10, visitedObject);
            try
            {
                BreakExecution();
                var holderIndex = _localReferences[visitedObject];
                Dump("holderIndex", holderIndex);
                _codeCache.IsValid = false;
                return ReturnMethodDump(CodeBase.LocalVariableReference(new Holder(holderIndex,ObjectId)));
            }
            finally
            {
                EndMethodDump();
            }
        }

        int ObtainHolderIndex(LocalReference visitedObject)
        {
            _data.Add(ReVisit(visitedObject) ?? visitedObject);
            return _data.Count - 1;
        }

        internal CodeBase LocalBlock(CodeBase body, CodeBase copier)
        {
            Tracer.Assert(!body.HasArg, body.Dump);
            var trace = ObjectId == -2;
            StartMethodDump(trace, body, copier);
            try
            {
                BreakExecution();
                var newBody = body.Visit(this) ?? body;
                var alignedBody = newBody.Align();
                var resultSize = alignedBody.Size;
                var alignedInternal = Code.Align();
                // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
                var gap = CodeBase.Void;
                Dump("newBody", newBody);
                Dump("aligned Body", alignedBody);
                Dump("alignedInternal", alignedInternal);
                BreakExecution();
                if(!copier.IsEmpty && alignedInternal.Size > Size.Zero && alignedInternal.Size < resultSize)
                    gap = CodeBase.BitsConst(resultSize - alignedInternal.Size, BitsConst.None());
                var statement = (alignedInternal + gap + alignedBody + DestructorCode)
                    .LocalBlockEnd(copier, resultSize);
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

        CodeBase ToLocalVariables(IEnumerable<CodeBase> codeBases)
            => CodeBase.List(codeBases.Select(LocalVariableDefinition));

        CodeBase LocalVariableDefinition(CodeBase value, int index)
            => value.Add(new LocalVariableDefinition(new Holder(index, ObjectId), value.Size));
    }
}