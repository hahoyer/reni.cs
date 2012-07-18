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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    sealed class ToNumberOfBaseFeature : ReniObject, ISuffixFeature, IMetaFunctionFeature
    {
        readonly TextItemsType _type;
        public ToNumberOfBaseFeature(TextItemsType type) { _type = type; }

        Result IMetaFunctionFeature.ApplyResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var target = left.Evaluate(context).ToString(_type.Parent.ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return TypeBase.Result(category, result)
                .Align(context.RefAlignParam.AlignBits);
        }

        IMetaFunctionFeature IFeature.MetaFunction { get { return this; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return null; } }
    }
}