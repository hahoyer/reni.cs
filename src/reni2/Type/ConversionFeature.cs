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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Type
{
    sealed class ConversionFeature : FunctionalFeature
    {
        readonly TypeBase _objectType;

        public ConversionFeature(TypeBase objectType) { _objectType = objectType; }

        internal override string DumpShort() { return _objectType.DumpShort() + " type"; }
        internal override Result ApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam)
        {
            return argsType
                .Conversion(category, _objectType);
        }
    }
}