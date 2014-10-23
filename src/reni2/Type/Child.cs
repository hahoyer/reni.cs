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
    abstract class Child<TParent> : TypeBase, IProxyType, ISimpleFeature
        where TParent : TypeBase
    {
        readonly TParent _parent;

        protected Child(TParent parent) { _parent = parent; }

        [DisableDump]
        internal override Root RootContext { get { return _parent.RootContext; } }
        [Node]
        [DisableDump]
        public TParent Parent { get { return _parent; } }

        ISimpleFeature IProxyType.Converter { get { return this; } }
        TypeBase ISimpleFeature.TargetType { get { return _parent; } }
        Result ISimpleFeature.Result(Category category) { return ParentConversionResult(category); }
        protected abstract Result ParentConversionResult(Category category);
    }
}