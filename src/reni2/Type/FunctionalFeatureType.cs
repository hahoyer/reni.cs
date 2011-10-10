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

namespace Reni.Type
{
    internal sealed class FunctionalFeatureType<TFeature> : TypeBase
        where TFeature : IFunctionalFeature
    {
        private readonly TFeature _functionalFeature;
        private readonly RefAlignParam _refAlignParam;

        internal FunctionalFeatureType(TFeature functionalFeature, RefAlignParam refAlignParam)
        {
            _functionalFeature = functionalFeature;
            _refAlignParam = refAlignParam;
        }

        internal override bool IsDataLess { get { return false; } }
        internal override Size GetSize() { return _refAlignParam.RefSize; }

        internal override string DumpShort() { return base.DumpShort() + "(" + _functionalFeature.DumpShort() + ")"; }

        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return _functionalFeature; } }
    }
}