using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    abstract class Child<TParent>
        : TypeBase
            , IProxyType
            , IConversion
            , IChild<TParent>
        where TParent : TypeBase
    {
        protected Child(TParent parent) { Parent = parent; }

        [Node]
        [DisableDump]
        internal readonly TParent Parent;

        [DisableDump]
        internal override Root Root => Parent.Root;
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


        TParent IChild<TParent>.Parent => Parent;
        IConversion IProxyType.Converter => this;
        TypeBase IConversion.Source => this;
        Result IConversion.Execute(Category category) => ParentConversionResult(category);

        protected abstract Result ParentConversionResult(Category category);
    }
}