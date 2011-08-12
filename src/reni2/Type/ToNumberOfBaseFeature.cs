//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class ToNumberOfBaseFeature : ISearchPath<IFeature, SequenceType>
    {
        private readonly DictionaryEx<int, ToNumberOfBaseSequenceFeature> _toNumberOfBaseSeqquenceFeaturesCache;
        private readonly TextItemType _type;
        public ToNumberOfBaseFeature(TextItemType type)
        {
            _toNumberOfBaseSeqquenceFeaturesCache = new DictionaryEx<int, ToNumberOfBaseSequenceFeature>(count => new ToNumberOfBaseSequenceFeature(_type, count));
            _type = type;
        }
        IFeature ISearchPath<IFeature, SequenceType>.Convert(SequenceType type)
        {
            Tracer.Assert(_type == type.Element);
            return _toNumberOfBaseSeqquenceFeaturesCache.Find(type.Count);
        }
    }

    internal sealed class ToNumberOfBaseSequenceFeature : TypeBase, IFeature, IMetaFeature
    {
        [EnableDump]
        private readonly TextItemType _type;
        [EnableDump]
        private readonly int _count;
        public ToNumberOfBaseSequenceFeature(TextItemType type, int count)
        {
            _type = type;
            _count = count;
        }
        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return Result(category); }
        TypeBase IFeature.ObjectType { get { return _type.UniqueSequence(_count); } }
        protected override Size GetSize() { return Size.Zero; }
        internal override IMetaFeature MetaFeature { get { return this; } }
        Result IMetaFeature.ObtainResult(Category category, ContextBase context, ICompileSyntax left, ICompileSyntax right, RefAlignParam refAlignParam)
        {
            var target = context.Evaluate(left).ToString(_type.Size);
            var conversionBase = context.Evaluate(right).ToInt32();
            Tracer.Assert(conversionBase >= 2 && conversionBase <= 36, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return UniqueNumber(result.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(result), Refs.ArgLess)
                .Align(refAlignParam.AlignBits);
        }
    }
}