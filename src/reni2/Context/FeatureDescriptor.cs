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
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    abstract class FeatureDescriptor : DumpableObject
    {
        internal abstract TypeBase Type { get; }
        protected abstract IFeatureImplementation Feature { get; }

        protected abstract Func<Category, Result> ConverterResult { get; }

        void AssertValid(ContextBase context, bool mustNotHaveCodeArgs, TypeBase objectType, CompileSyntax right)
        {
            Tracer.Assert(Feature != null);

            if(Feature.MetaFunction != null)
                return;

            if(mustNotHaveCodeArgs)
                Tracer.Assert(!Feature.HasCodeArgs(context, objectType, ConverterResult, right));

            if(right != null)
            {
                Tracer.Assert(Feature.Function != null, Feature.Dump);
                Tracer.Assert(Feature.Function == null || !Feature.Function.IsImplicit, Feature.Dump);
                return;
            }

            if(Feature.Simple != null)
            {
                Tracer.Assert
                    (
                        Feature.Function == null || !Feature.Function.IsImplicit,
                        () => "Ambiguity: Simple or ImplicitFunction? " + Feature.Dump());
                return;
            }

            Tracer.Assert(Feature.Function != null, Feature.Dump);
            Tracer.Assert(Feature.Function.IsImplicit, Feature.Dump);
        }

        Result ApplyResult(Category category, ContextBase context, CompileSyntax left, CompileSyntax right)
        {
            var trace = context.ObjectId == 1 && category.HasArgs && right == null && left != null && left.ObjectId == 152;
            StartMethodDump(trace, category, context, left, right);
            try
            {
                var resultFromObject = context.Result(category, Feature, Type, right);

                Dump("resultFromObject", resultFromObject);
                if(trace)
                    Dump("ConverterResult", ConverterResult(Category.All));

                var convertedResult = resultFromObject.ReplaceArg(ConverterResult);
                Tracer.Assert(category == convertedResult.CompleteCategory);

                Dump("convertedResult", convertedResult);
                if(trace)
                    Dump("objectResult", context.ObjectResult(Category.All, left));
                BreakExecution();

                var result = convertedResult.ReplaceArg(c => context.ObjectResult(c, left));
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
        /// <summary>
        ///     Obtains the feature result.
        ///     Special case of a meta feature is treated here.
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="category"> the categories in result </param>
        /// <param name="context"> </param>
        /// <param name="left"> </param>
        /// <param name="right"> </param>
        /// <returns> </returns>
        internal Result Result(Category category, ContextBase context, CompileSyntax left, CompileSyntax right)
        {
            AssertValid(context, category.HasArgs && left == null, Type, right);

            var metaFeature = Feature.MetaFunction;
            if(metaFeature != null)
                return metaFeature.ApplyResult(context, category, left, right);

            return ApplyResult(category, context, left, right);
        }
    }

    sealed class CallDescriptor : FeatureDescriptor
    {
        readonly TypeBase _definingType;
        readonly IFeatureImplementation _feature;
        readonly Func<Category, Result> _converterResult;
        public CallDescriptor(TypeBase definingType, IFeatureImplementation feature, Func<Category, Result> converterResult)
        {
            _definingType = definingType;
            _feature = feature;
            _converterResult = converterResult;
        }
        internal override TypeBase Type
        {
            get
            {
                var result = ConverterResult(Category.Type);
                return result != null ? result.Type : _definingType;
            }
        }
        protected override IFeatureImplementation Feature { get { return _feature; } }
        protected override Func<Category, Result> ConverterResult { get { return _converterResult; } }
    }

    sealed class FunctionalObjectDescriptor : FeatureDescriptor
    {
        readonly ContextBase _context;
        readonly CompileSyntax _left;
        internal FunctionalObjectDescriptor(ContextBase context, CompileSyntax left)
        {
            _context = context;
            _left = left;
        }
        internal override TypeBase Type { get { return _context.Type(_left); } }
        protected override Func<Category, Result> ConverterResult { get { return null; } }
        protected override IFeatureImplementation Feature { get { return Type.Feature; } }
    }

    sealed class FunctionalArgDescriptor : FeatureDescriptor
    {
        readonly ContextBase _context;
        internal FunctionalArgDescriptor(ContextBase context) { _context = context; }

        [DisableDump]
        FunctionBodyType FunctionBodyType { get { return (FunctionBodyType) _context.ArgReferenceResult(Category.Type).Type; } }
        [DisableDump]
        Structure Structure { get { return FunctionBodyType.FindRecentStructure; } }

        [DisableDump]
        protected override Func<Category, Result> ConverterResult { get { return Structure.StructReferenceViaContextReference; } }
        [DisableDump]
        internal override TypeBase Type { get { return Structure.Type; } }
        [DisableDump]
        protected override IFeatureImplementation Feature { get { return FunctionBodyType.Feature; } }
    }
}