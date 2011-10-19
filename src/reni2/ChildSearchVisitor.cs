// 
//     Project Reni2
//     Copyright (C) 2011 - 2011 Harald Hoyer
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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni
{
    sealed class ChildSearchVisitor<TFeature, TType> : SearchVisitor<ISearchPath<TFeature, TType>>
        where TFeature : class
        where TType : IDumpShortProvider
    {
        [DisableDump]
        readonly SearchVisitor<TFeature> _parent;

        [EnableDump]
        readonly TType _target;

        public ChildSearchVisitor(SearchVisitor<TFeature> parent, TType target)
        {
            _parent = parent;
            _target = target;
        }

        internal override bool IsSuccessFull { get { return _parent.IsSuccessFull; } }
        internal override Defineable Defineable { get { return _parent.Defineable; } }

        internal override ISearchPath<TFeature, TType> InternalResult
        {
            set
            {
                if(value != null)
                    _parent.InternalResult = value.Convert(_target);
            }
        }
        internal override ConversionFunction[] ConversionFunctions { get { return _parent.ConversionFunctions; } set { _parent.ConversionFunctions = value; } }
    }
}