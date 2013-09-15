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
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class AccessFeature : DumpableObject, ISimpleFeature, IFeatureImplementation
    {
        static int _nextObjectId;

        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly int _position;
        readonly ValueCache<IFunctionFeature > _functionFeature;

        internal AccessFeature(Structure structure, int position)
            : base(_nextObjectId++)
        {
            _structure = structure;
            _position = position;
            _functionFeature = new ValueCache<IFunctionFeature>(ObtainFunctionFeature);
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _structure
                .AccessViaThisReference(category, _position);
        }

        CompileSyntax Statement
        {
            get
            {
                return _structure
                    .ContainerContextObject
                    .Container
                    .Statements[_position];
            }
        }

        IMetaFunctionFeature IFeatureImplementation.MetaFunction
        {
            get
            {
                var syntax = Statement as FunctionSyntax;
                return syntax == null ? null : syntax.MetaFunctionFeature(_structure);
            }
        }

        IFunctionFeature IFeatureImplementation.Function { get { return FunctionFeature; } }
        IFunctionFeature FunctionFeature { get { return _functionFeature.Value; } }

        IFunctionFeature ObtainFunctionFeature()
        {
            var functionSyntax = Statement as FunctionSyntax;
            if(functionSyntax != null)
                return functionSyntax.FunctionFeature(_structure);

            var feature = _structure.ValueType(_position).Feature;
            if(feature == null)
                return null;
            return feature.Function;
        }

        ISimpleFeature IFeatureImplementation.Simple
        {
            get
            {
                var function = FunctionFeature;
                if(function != null && function.IsImplicit)
                    return null;
                return this;
            }
        }
    }
}