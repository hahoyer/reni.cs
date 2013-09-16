#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using hw.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    abstract class FunctionalFeature : DumpableObject
    {
        static int _nextObjectId;
        readonly ValueCache<TypeBase> _functionalTypesCache;

        protected FunctionalFeature()
            : base(_nextObjectId++)
        {
            _functionalTypesCache
                = new ValueCache<TypeBase>(() => new FunctionFeatureType(this));
        }

        [DisableDump]
        internal abstract IContextReference ObjectReference { get; }
        [DisableDump]
        internal abstract Root RootContext { get; }

        internal TypeBase UniqueFunctionalType() { return _functionalTypesCache.Value; }
        internal abstract Result ApplyResult(Category category, TypeBase argsType);
    }
}