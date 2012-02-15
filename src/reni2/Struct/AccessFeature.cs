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
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    sealed class AccessFeature :
        ReniObject,
        ISuffixFeature
    {
        [EnableDump]
        readonly Structure _structure;

        [EnableDump]
        readonly int _position;

        internal AccessFeature(Structure structure, int position)
        {
            _structure = structure;
            _position = position;
        }

        TypeBase IFeature.ObjectType { get { return _structure.Type; } }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return _structure.AccessViaThisReference(category, _position); }
    }


    sealed class ContextAccessFeature :
        ReniObject,
        IContextFeature
    {
        [EnableDump]
        readonly Structure _structure;

        [EnableDump]
        readonly int _position;

        internal ContextAccessFeature(Structure structure, int position)
        {
            _structure = structure;
            _position = position;
        }

        TypeBase IFeature.ObjectType { get { return _structure.Type; } }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return _structure.AccessViaContextReference(category, _position); }
    }
}