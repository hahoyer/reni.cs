using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    abstract class Child<TParent>
        : TypeBase
            , IProxyType
            , IValueFeature
        where TParent : TypeBase
    {
        protected Child(TParent parent) { Parent = parent; }

        [Node]
        [DisableDump]
        public TParent Parent { get; }
        [DisableDump]
        internal override Root RootContext => Parent.RootContext;

        IValueFeature IProxyType.Converter => this;
        TypeBase IValueFeature.TargetType => Parent;
        Result IValueFeature.Result(Category category) => ParentConversionResult(category);

        protected abstract Result ParentConversionResult(Category category);
    }
}