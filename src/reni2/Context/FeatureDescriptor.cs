using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.Type;

namespace Reni.Context
{
    abstract class FeatureDescriptor : DumpableObject
    {
        [DisableDump]
        protected abstract TypeBase Type { get; }
        [DisableDump]
        protected abstract IFeatureImplementation Feature { get; }

        [DisableDump]
        protected abstract Func<Category, Result> ConverterResult { get; }

        internal Result Result(Category category, ContextBase context, CompileSyntax left, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, left, right);

            var trace = ObjectId == 37 && category.HasCode;
            StartMethodDump(trace, context, category, left, right);
            try
            {
                BreakExecution();
                var rawResult = Result(context, category, right);
                var converterResult = ConverterResult(category.Typed);
                Dump("rawResult", rawResult);
                Dump("converterResult", converterResult);
                BreakExecution();
                var resultWithConversions = rawResult.ReplaceArg(ConverterResult);
                Dump("resultWithConversions", resultWithConversions);
                BreakExecution();
                var objectResult = context.ObjectResult(category, left);
                Dump("objectResult", objectResult);
                BreakExecution();
                var result = resultWithConversions.ReplaceArg(objectResult);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Result(Category category, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            var result = Result(context, category, right);
            return result.ReplaceArg(ConverterResult);
        }

        internal Result Result(Category category) => SimpleResult(category).ReplaceArg(ConverterResult);

        Result Result(ContextBase context, Category category, CompileSyntax right)
        {
            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null && right == null)
                return simpleFeature.Result(category);

            var trace = ObjectId == -55 && category.HasCode;
            StartMethodDump(trace, context, category, right);
            try
            {
                var function = Feature.Function;
                var argsType = context.ArgsResult(Category.Type, right).Type;
                Dump("argsType", argsType);
                BreakExecution();

                var applyResult = function
                    .ApplyResult(category, argsType);
                Dump("applyResult", applyResult);
                BreakExecution();

                var replaceArg = applyResult
                    .ReplaceArg(c => context.ArgsResult(c, right));
                Dump("replaceArg", replaceArg);
                BreakExecution();

                var result = replaceArg
                    .ReplaceAbsolute(function.ObjectReference, c => Type.ArgResult(c.Typed).LocalPointerKindResult);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result SimpleResult(Category category)
        {
            Tracer.Assert(Feature.Meta == null);

            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null)
                return simpleFeature.Result(category);

            var function = Feature.Function;
            var result = function
                .ApplyResult(category, Type.RootContext.VoidType)
                .ReplaceAbsolute(function.ObjectReference, Type.SmartPointer.ArgResult);
            return result;
        }
    }

    sealed class CallDescriptor : FeatureDescriptor
    {
        readonly TypeBase _definingType;
        public CallDescriptor(TypeBase definingType, IFeatureImplementation feature, Func<Category, Result> converterResult)
        {
            _definingType = definingType;
            Feature = feature;
            ConverterResult = converterResult;
            StopByObjectId(-41);
        }
        [DisableDump]
        protected override TypeBase Type
        {
            get
            {
                var result = ConverterResult(Category.Type);
                return result != null ? result.Type : _definingType;
            }
        }
        [DisableDump]
        protected override IFeatureImplementation Feature { get; }
        [DisableDump]
        protected override Func<Category, Result> ConverterResult { get; }
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
        protected override TypeBase Type => _context.Type(_left);
        protected override Func<Category, Result> ConverterResult => null;
        protected override IFeatureImplementation Feature => Type.Feature;
    }

    sealed class FunctionalArgDescriptor : FeatureDescriptor
    {
        readonly ContextBase _context;
        internal FunctionalArgDescriptor(ContextBase context) { _context = context; }

        [DisableDump]
        FunctionBodyType FunctionBodyType => (FunctionBodyType) _context.ArgReferenceResult(Category.Type).Type;
        [DisableDump]
        CompoundView CompoundView => FunctionBodyType.FindRecentCompoundView;

        [DisableDump]
        protected override Func<Category, Result> ConverterResult => CompoundView.StructReferenceViaContextReference;
        [DisableDump]
        protected override TypeBase Type => CompoundView.Type;
        [DisableDump]
        protected override IFeatureImplementation Feature => FunctionBodyType.Feature;
    }
}