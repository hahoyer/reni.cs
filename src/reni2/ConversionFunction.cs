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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni
{
    sealed class ConversionFunction : ReniObject, IConversionFunction
    {
        static int _nextObjectId;
        readonly IContainerType _containerType;

        internal ConversionFunction(IContainerType containerType)
            : base(_nextObjectId++) { _containerType = containerType; }
        Result IConversionFunction.Result(Category category) { return _containerType.Converter().Result(category); }
        public override string NodeDump
        {
            get
            {
                return base.NodeDump 
                    + "[" 
                    + ((ReniObject) _containerType).NodeDump 
                    + "=>" 
                    + _containerType.Target.NodeDump 
                    + "]";
            }
        }
    }

    interface IContainerType
    {
        IConverter Converter();
        TypeBase Target { get; }
    }
}