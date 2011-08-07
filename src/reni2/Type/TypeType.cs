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

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        [EnableDump]
        private readonly TypeBase _value;

        private readonly IFunctionalFeature _functionalFeature;
        private readonly RepeaterType _repeaterType;

        public TypeType(TypeBase value)
        {
            _repeaterType = new RepeaterType(this);
            _functionalFeature = new ConversionFeature(value.AutomaticDereference());
            _value = value;
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }

        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return _functionalFeature; } }

        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }
        [DisableDump]
        internal override TypeBase ObjectType { get { return _value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam) { return Void.Result(category, () => CodeBase.DumpPrintText(_value.DumpPrintText)); }
        internal Result Repeat(Category category, RefAlignParam refAlignParam) { return _repeaterType.Result(category); }


        private sealed class RepeaterType : TypeBase, IFunctionalFeature
        {
            internal override IFunctionalFeature FunctionalFeature { get { return this; } }
            private readonly TypeType _typeType;
            public RepeaterType(TypeType typeType) { _typeType = typeType; }
            protected override Size GetSize() { return Size.Zero; }
            
            Result IFunctionalFeature.ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
            {
                var count = argsResult
                    .AutomaticDereference()
                    .Evaluate()
                    .ToInt32();
                return _typeType
                    .ObjectType
                    .UniqueAlign(refAlignParam.AlignBits)
                    .UniqueArray(count)
                    .UniqueTypeType
                    .Result(category);
            }
            bool IFunctionalFeature.IsRegular { get { return false; } }
        }
    }
}