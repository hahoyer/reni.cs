#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    sealed class FeatureInstance : ReniObject
    {
        readonly TypeBase _objectType;
        readonly IFeature _feature;
        readonly Func<Category, Result> _converterResult;
        readonly bool _hasNoArg;

        internal FeatureInstance(TypeBase objectType, IFeature feature, Func<Category, Result> converterResult, bool hasNoArg)
        {
            _objectType = objectType;
            _feature = feature;
            _converterResult = converterResult;
            _hasNoArg = hasNoArg;
            AssertValid();
        }

        ISimpleFeature Simple
        {
            get
            {
                if(!_hasNoArg)
                    return null;

                var function = _feature.Function;
                if(function != null && function.IsImplicit)
                    return null;

                return _feature.Simple;
            }
        }

        void AssertValid()
        {
            Tracer.Assert(_feature != null);

            if(_feature.MetaFunction != null)
                return;

            if(!_hasNoArg)
            {
                Tracer.Assert(_feature.Function != null, _feature.Dump);
                Tracer.Assert(!_feature.Function.IsImplicit, _feature.Dump);
                return;
            }

            if(_feature.Simple != null)
            {
                Tracer.Assert(_feature.Function == null || !_feature.Function.IsImplicit, () => "Ambiguity: Simple or ImplicitFunction? " + _feature.Dump());
                return;
            }

            Tracer.Assert(_feature.Function != null, _feature.Dump);
            Tracer.Assert(_feature.Function.IsImplicit, _feature.Dump);
        }

        /// <summary>
        ///     Obtains the feature result (Meta feature is not expeced here). 
        ///     Actual arguments, if provided, are replaced. 
        ///     The object reference will appear as CodeBase.Arg
        /// </summary>
        /// <param name="category"> the categories in result </param>
        /// <param name="getArgs"> the function that provides the categories for the actual arguments. If treating a simple feature, this is not used </param>
        /// <returns> </returns>
        Result Result(Category category, Func<Category, Result> getArgs)
        {
            if(Simple == null)
            {
                var applyResult = ApplyResult(category, getArgs);
                Tracer.Assert(category == applyResult.CompleteCategory);
                return applyResult;
            }

            var result = Simple.Result(category);
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        Result ApplyResult(Category category, Func<Category, Result> getArgs)
        {
            var function = _feature.Function;
            var args = new ResultCache(getArgs);
            var applyResult = function.ApplyResult(category, args.Type);
            Tracer.Assert(category == applyResult.CompleteCategory);
            return applyResult
                .ReplaceArg(args)
                .ReplaceAbsolute(function.ObjectReference, c => _objectType.SmartPointer.ArgResult(c));
        }

        /// <summary>
        ///     Obtains the feature result. 
        ///     Special case of a meta feature is treated here.
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="context"> the context where the feature access is located. Used to compile the syntax elements provided </param>
        /// <param name="category"> the categories in result </param>
        /// <param name="left"> the expression left to the feature access, if provided </param>
        /// <param name="right"> the expression right to the feature access, if provided </param>
        /// <returns> </returns>
        internal Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            Tracer.Assert(_hasNoArg == (right == null));

            var metaFeature = _feature.MetaFunction;
            if(metaFeature != null)
                return metaFeature.ApplyResult(context, category, left, right);

            var rawResult = Result(category, c => context.ResultForArgs(c, right));
            Tracer.Assert(category == rawResult.CompleteCategory);
            return rawResult
                .ReplaceArg(_converterResult)
                .ReplaceArg(c => context.ResultForObject(c, left));
        }

        /// <summary>
        ///     Obtains the feature result of a functional object. 
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="context"> the context where the feature access is located. Used to compile the syntax elements provided </param>
        /// <param name="category"> the categories in result </param>
        /// <param name="left"> the expression left to the feature access, if provided </param>
        /// <param name="right"> the expression right to the feature access, if provided </param>
        /// <returns> </returns>
        internal static Result ObjectResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var leftType = context.Type(left);
            return new FeatureInstance(leftType, leftType.Feature, null, right == null)
                .Result(context, category, left, right);
        }
    }
}