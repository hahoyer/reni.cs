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
        public RecursionType(Root root) { Root = root; }
        internal override Root Root { get; }
        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }


        IMeta IMetaImplementation.Function => this;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => this;
        bool IFunction.IsImplicit => false;
        int IContextReference.Order => ObjectId;

        Result IFunction.Result(Category category, TypeBase argsType)
        {
            NotImplementedMethod(category, argsType);
            return null;
        }

        Result IValue.Execute(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        Result IMeta.Result
            (Category category, ResultCache left, ContextBase contextBase, Syntax right)
        {
            NotImplementedMethod(contextBase, left, category, right);
            return null;
        }


    }
}