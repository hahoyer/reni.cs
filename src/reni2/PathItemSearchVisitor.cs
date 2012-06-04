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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;

namespace Reni
{
    sealed class PathItemSearchVisitor<TFeature, TType> : SearchVisitor<ISearchPath<TFeature, TType>>
        where TFeature : class, ISearchPath
        where TType : IDumpShortProvider
    {
        [DisableDump]
        readonly SearchVisitor<TFeature> _parent;

        [EnableDump]
        readonly TType _target;

        public PathItemSearchVisitor(SearchVisitor<TFeature> parent, TType target)
            : base(parent.Probe)
        {
            _parent = parent;
            _target = target;
        }

        internal override bool IsSuccessFull { get { return _parent.IsSuccessFull; } }
        internal override ISearchTarget Target { get { return _parent.Target; } }

        internal override ISearchPath<TFeature, TType> InternalResult
        {
            set
            {
                if(value != null)
                    _parent.InternalResult = value.Convert(_target);
            }
        }
        internal override IConversionFunction[] ConversionFunctions { get { return _parent.ConversionFunctions; } set { _parent.ConversionFunctions = value; } }
    }
}