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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        [EnableDump]
        private readonly TypeBase _value;

        private readonly ConversionFeature _functionalFeature;
        private readonly RepeaterType _repeaterType;

        public TypeType(TypeBase value)
        {
            _repeaterType = new RepeaterType(this);
            _functionalFeature = new ConversionFeature(value.AutomaticDereferenceType);
            _value = value;
        }

        internal override bool IsDataLess { get { return true; } }
        
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }

        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }
        [DisableDump]
        private TypeBase Value { get { return _value; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.SearchAtPath(this);
            base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category)
        {
            return Void
                .Result
                (category
                 , () => CodeBase.DumpPrintText(_value.DumpPrintText)
                 , CodeArgs.Void
                );
        }
        internal Result Repeat(Category category) { return _repeaterType.Result(category); }

        private sealed class RepeaterType : TypeBase, IMetaFeature
        {
            private readonly TypeType _typeType;
            public RepeaterType(TypeType typeType) { _typeType = typeType; }

            internal override bool IsDataLess { get { return true; } }
            
            Result IMetaFeature.ApplyResult(Category category, ContextBase contextBase, CompileSyntax left, CompileSyntax right, RefAlignParam refAlignParam)
            {
                var count = right
                    .Result(contextBase)
                    .AutomaticDereference()
                    .Evaluate(contextBase.RootContext.OutStream)
                    .ToInt32();
                return _typeType
                    .Value
                    .UniqueAlign(refAlignParam.AlignBits)
                    .UniqueArray(count)
                    .UniqueTypeType
                    .Result(category);
            }
        }
    }
}