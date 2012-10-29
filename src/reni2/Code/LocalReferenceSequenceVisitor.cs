﻿#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code.ReplaceVisitor;

namespace Reni.Code
{
    sealed class LocalReferenceSequenceVisitor : Base
    {
        readonly SimpleCache<CodeBase> _codeCache;

        [Node]
        [EnableDump]
        readonly List<LocalReference> _data = new List<LocalReference>();
        [Node]
        [EnableDump]
        readonly DictionaryEx<LocalReference, int> _localReferences;

        static int _nextObjectId;

        public LocalReferenceSequenceVisitor()
            : base(_nextObjectId++)
        {
            _localReferences = new DictionaryEx<LocalReference, int>(-1, ObtainHolderIndex);
            _codeCache = new SimpleCache<CodeBase>(Convert);
        }

        CodeBase Convert()
        {
            return _data
                .Select(localReference => localReference.Code)
                .ToLocalVariables(HolderNamePattern);
        }

        [DisableDump]
        CodeBase Code { get { return _codeCache.Value; } }

        CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return _data
                    .Select((localReference, i) => localReference.AccompayningDestructorCode(ref size, HolderName(i)))
                    .ToSequence();
            }
        }

        string HolderNamePattern { get { return "h_" + ObjectId + "_{0}"; } }

        internal override CodeBase LocalReference(LocalReference visitedObject)
        {
            StartMethodDump(ObjectId == -10, visitedObject);
            try
            {
                BreakExecution();
                var holderIndex = _localReferences[visitedObject];
                Dump("holderIndex", holderIndex);
                _codeCache.Reset();
                return ReturnMethodDump(CodeBase.LocalVariableReference(HolderName(holderIndex)));
            }
            finally
            {
                EndMethodDump();
            }
        }
        string HolderName(int holderIndex) { return string.Format(HolderNamePattern, holderIndex); }

        int ObtainHolderIndex(LocalReference visitedObject)
        {
            _data.Add(ReVisit(visitedObject) ?? visitedObject);
            return _data.Count - 1;
        }

        internal CodeBase LocalBlock(CodeBase body, CodeBase copier)
        {
            Tracer.Assert(!body.HasArg, body.Dump);
            var trace = ObjectId == 2;
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
    }
}