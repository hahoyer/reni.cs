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
using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni
{
    sealed class TypeSearchResult : SearchResult
    {
        readonly TypeBase _type;

        internal TypeSearchResult(ITypeFeature feature, ConversionFunction[] conversionFunctions, TypeBase type)
            : base(feature, conversionFunctions) { _type = type; }

        void AssertValid(RefAlignParam refAlignParam)
        {
            if(ConversionFunctions.Length == 0)
                return;
            Tracer.Assert(_type.SmartReference(refAlignParam) == ConversionFunctions[ConversionFunctions.Length - 1].ArgType);
        }

        protected override Result TrivialConversionResult(Category category, RefAlignParam refAlignParam) { return _type.SmartReference(refAlignParam).ArgResult(category); }
        
        protected override Result ConverterResult(Category category)
        {
            var trace = ObjectId == -4;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var result = ConversionFunctions[0].Result(category);
                for(var i = 1; i < ConversionFunctions.Length; i++)
                    result = result.ReplaceArg(ConversionFunctions[i].Result);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}