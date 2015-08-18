using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser
{
    sealed class RecursionType
        : TypeBase
            , IFeatureImplementation
            , IFunctionFeature
            , IValueFeature
            , IContextMetaFunctionFeature
            , IMetaFunctionFeature
            , IContextReference
    {
        public RecursionType(Root rootContext) { RootContext = rootContext; }
        internal override Root RootContext { get; }

        IMetaFunctionFeature IFeatureImplementation.Meta => this;
        IFunctionFeature IFeatureImplementation.Function => this;
        IValueFeature IFeatureImplementation.Value => this;
        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => this;
        bool IFunctionFeature.IsImplicit => false;
        IContextReference IFunctionFeature.ObjectReference => this;
        TypeBase IValueFeature.TargetType => this;
        int IContextReference.Order => ObjectId;

        Result IFunctionFeature.Result(Category category, TypeBase argsType)
        {
            NotImplementedMethod(category, argsType);
            return null;
        }

        Result IValueFeature.Result(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        Result IContextMetaFunctionFeature.Result
            (ContextBase contextBase, Category category, CompileSyntax right)
        {
            NotImplementedMethod(contextBase, category, right);
            return null;
        }

        Result IMetaFunctionFeature.Result
            (Category category, ResultCache left, ContextBase contextBase, CompileSyntax right)
        {
            NotImplementedMethod(contextBase, left, category, right);
            return null;
        }

        Size IContextReference.Size
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

    }
}