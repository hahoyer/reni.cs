﻿#region Copyright (C) 2012

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
using Reni.Context;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    sealed class StructReferenceFeature
        : ReniObject
          , ISearchPath<ISuffixFeature, PointerType>
          , ISuffixFeature
          , ISimpleFeature
    {
        [EnableDump]
        readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        ISuffixFeature ISearchPath<ISuffixFeature, PointerType>.Convert(PointerType type)
        {
            Tracer.Assert(Root.DefaultRefAlignParam == _structureType.RefAlignParam);
            return this;
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _structureType
                .Structure
                .DumpPrintResultViaContextReference(category);
        }
        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
    }
}