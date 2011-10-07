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

using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    sealed class ContextSearchVisitor : RootSearchVisitor<IContextFeature>
    {
        internal ContextSearchVisitor(Defineable defineable)
            : base(defineable) { }

        internal SearchResult SearchResult
        {
            get
            {
                if(IsSuccessFull)
                    return new ContextSearchResult(Result, ConversionFunctions);
                return null;
            }
        }

        internal void Search(ContextBase contextBase)
        {
            if(IsSuccessFull)
                return;
            contextBase.Search(this);
        }
        protected override void AssertValid()
        {
            TypeBase lastType = null;
            foreach(var currentType in ConversionFunctions)
            {
                Tracer.Assert(lastType == currentType.ArgType);
                lastType = currentType.ResultType;
            }
        }
        internal void Search(Structure accessPoint, ConversionFunction conversionFunction)
        {
            var feature = accessPoint.SearchFromStructContext(Defineable);
            if(feature == null)
                return;
            InternalResult = feature.ConvertToContextFeature(accessPoint);
            Add(conversionFunction);
        }
    }

    sealed class ContextSearchResult : SearchResult
    {
        internal ContextSearchResult(IContextFeature feature, ConversionFunction[] conversionFunctions)
            : base(feature, conversionFunctions) { }

        protected override Result TrivialConversionResult(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, refAlignParam);
            return null;
        }
    }
}