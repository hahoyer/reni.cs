// 
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
    sealed class ToNumberOfBaseFeature : ISearchPath<ISuffixFeature, SequenceType>
    {
        readonly DictionaryEx<int, ToNumberOfBaseSequenceFeature> _toNumberOfBaseSeqquenceFeaturesCache;
        readonly TextItemType _type;
        public ToNumberOfBaseFeature(TextItemType type)
        {
            _toNumberOfBaseSeqquenceFeaturesCache = new DictionaryEx<int, ToNumberOfBaseSequenceFeature>(count => new ToNumberOfBaseSequenceFeature(_type, count));
            _type = type;
        }
        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type)
        {
            Tracer.Assert(_type == type.Element);
            return _toNumberOfBaseSeqquenceFeaturesCache.Find(type.Count);
        }
    }

    sealed class ToNumberOfBaseSequenceFeature : TypeBase, ISuffixFeature, IMetaFeature
    {
        [EnableDump]
        readonly TextItemType _type;
        [EnableDump]
        readonly int _count;
        public ToNumberOfBaseSequenceFeature(TextItemType type, int count)
        {
            _type = type;
            _count = count;
        }
        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return Result(category); }
        internal override bool IsDataLess { get { return true; } }
        Result IMetaFeature.ApplyResult(Category category, ContextBase context, CompileSyntax left, CompileSyntax right, RefAlignParam refAlignParam)
        {
            var target = left.Evaluate(context).ToString(_type.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2 && conversionBase <= 36, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return UniqueNumber(result.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(result), CodeArgs.Void)
                .Align(refAlignParam.AlignBits);
        }
    }
}