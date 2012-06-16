#region Copyright (C) 2012

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
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    sealed class Void : TypeBase
    {
        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }
        protected override ISuffixFeature Convert(TypeBase sourceType) { return sourceType.IsDataLess ? Extension.Feature(c => Result(c, sourceType.ArgResult)) : null; }
        protected override TypeBase ReversePair(TypeBase first) { return first; }
        [DisableDump]
        internal override bool IsDataLess { get { return true; } }
        internal override TypeBase Pair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        internal override string DumpShort() { return "void"; }
        internal Result DumpPrintResult(Category category) { return Result(category); }
    }
}