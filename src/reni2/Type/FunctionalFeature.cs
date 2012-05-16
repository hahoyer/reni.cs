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

namespace Reni.Type
{
    abstract class FunctionalFeature : ReniObject, IDumpShortProvider
    {
        static int _nextObjectId;
        readonly DictionaryEx<RefAlignParam, TypeBase> _functionalTypesCache;

        protected FunctionalFeature()
            : base(_nextObjectId++)
        {
            _functionalTypesCache
                = new DictionaryEx<RefAlignParam, TypeBase>(refAlignParam => new FunctionalFeatureType(this, refAlignParam));
        }

        internal TypeBase UniqueFunctionalType(RefAlignParam refAlignParam) { return _functionalTypesCache.Find(refAlignParam); }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        internal abstract Result ApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam);
        internal abstract Result ReplaceObjectReference(Result result, Result objectResult, RefAlignParam refAlignParam);
    }
}