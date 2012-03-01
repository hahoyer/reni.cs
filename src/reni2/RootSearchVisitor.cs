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
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni
{
    abstract class RootSearchVisitor<TFeature> : SearchVisitor<TFeature>
        where TFeature : class, IFeature
    {
        readonly Defineable _defineable;
        protected TFeature Result { get; private set; }

        internal RootSearchVisitor(Defineable defineable)
        {
            _defineable = defineable;
            ConversionFunctions = new IConversionFunction[0];
        }

        internal override sealed IConversionFunction[] ConversionFunctions { get; set; }
        internal override bool IsSuccessFull { get { return Result != null; } }

        internal override TFeature InternalResult
        {
            set
            {
                Tracer.Assert(Result == null || Result == value);
                Result = value;
            }
        }

        internal override Defineable Defineable { get { return _defineable; } }
    }
}