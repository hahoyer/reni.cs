#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2012 - 2013 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Feature
{
    sealed class MetaFunction : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        readonly Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> _function;
        public MetaFunction(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function) { _function = function; }

        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return this; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }

        Result IMetaFunctionFeature.ApplyResult(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right) { return _function(contextBase, category, left, right); }
    }
}