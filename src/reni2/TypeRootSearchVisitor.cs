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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    sealed class TypeRootSearchVisitor<TFeature> : RootSearchVisitor<TFeature>
        where TFeature : class, ITypeFeature
    {
        [EnableDump]
        readonly TypeBase _type;

        internal TypeRootSearchVisitor(Defineable defineable, TypeBase type)
            : base(defineable) { _type = type; }

        protected override void AssertValid()
        {
            if (ConversionFunctions.Length == 0)
                return;
            var lastType = Result.ObjectType;
            var result = true;
            for(var index = 0; index < ConversionFunctions.Length; index++)
            {
                var currentType = ConversionFunctions[index];
                if (index == 0 && lastType != currentType.ResultType && currentType.ResultType is AutomaticReferenceType)
                    lastType = lastType.SmartReference(((AutomaticReferenceType)currentType.ResultType).RefAlignParam);
                if(lastType != currentType.ResultType)
                {
                    result = false;
                    Tracer.FlaggedLine("Type mismatch at position " + index);
                }
                lastType = currentType.ArgType;
            }

            Tracer.Assert(result, Dump());
        }

        internal SearchResult SearchResult
        {
            get
            {
                if(IsSuccessFull)
                    return new TypeSearchResult(Result, ConversionFunctions, _type);
                return null;
            }
        }
    }
}