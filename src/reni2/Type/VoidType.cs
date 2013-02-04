#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;

namespace Reni.Type
{
    sealed class VoidType : TypeBase
        , IFeaturePath<ISuffixFeature, DumpPrintToken>
    {
        readonly Root _rootContext;
        public VoidType(Root rootContext) { _rootContext = rootContext; }
        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }
        ISuffixFeature IFeaturePath<ISuffixFeature, DumpPrintToken>.GetFeature(DumpPrintToken target) { return Extension.Feature(DumpPrintTokenResult); }
        protected override ISuffixFeature Convert(TypeBase sourceType) { return sourceType.IsDataLess ? Extension.Feature(c => Result(c, sourceType.ArgResult)) : null; }
        protected override TypeBase ReversePair(TypeBase first) { return first; }
        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }
        [DisableDump]
        internal override bool IsDataLess { get { return true; } }
        internal override TypeBase Pair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        protected override string GetNodeDump() { return "void"; }
        internal Result DumpPrintTokenResult(Category category) { return Result(category); }
    }
}