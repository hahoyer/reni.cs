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
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal sealed class DumpPrintToken :
        Defineable,
        IFeature,
        ISearchPath<ISearchPath<IFeature, AutomaticReferenceType>, StructureType>,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<IFeature, StructureType>,
        ISearchPath<ISearchPath<IFeature, BaseType>, Bit>,
        ISearchPath<IFeature, FunctionalBody>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return new Feature(type.DumpPrintResult); }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new Feature(type.Result); }
        IFeature ISearchPath<IFeature, StructureType>.Convert(StructureType type) { return new Feature(type.DumpPrintResult); }

        IFeature ISearchPath<IFeature, FunctionalBody>.Convert(FunctionalBody type) { return new Feature(type.DumpPrintResult); }

        ISearchPath<IFeature, BaseType> ISearchPath<ISearchPath<IFeature, BaseType>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, AutomaticReferenceType> ISearchPath<ISearchPath<IFeature, AutomaticReferenceType>, StructureType>.Convert(StructureType type) { return type.DumpPrintReferenceFeature; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, refAlignParam);
            return null;
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        internal static DumpPrintToken Create() { return new DumpPrintToken {Name = "<dump_print>"}; }
    }
}