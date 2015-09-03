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
            , IImplementation
            , IFunction
            , IValue
            , IMeta
            , IContextReference
    {
        public RecursionType(Root rootContext) { RootContext = rootContext; }
        internal override Root RootContext { get; }

        IMeta IMetaImplementation.Function => this;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => this;
        bool IFunction.IsImplicit => false;
        TypeBase IValue.Source => this;
        int IContextReference.Order => ObjectId;

        Result IFunction.Result(Category category, TypeBase argsType)
        {
            NotImplementedMethod(category, argsType);
            return null;
        }

        Result IValue.Result(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        Result IMeta.Result
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