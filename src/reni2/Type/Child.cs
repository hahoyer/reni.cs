using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    abstract class Child<TParent> : TypeBase, IProxyType, IConverter
        where TParent : TypeBase
    {
        readonly TParent _parent;

        protected Child(TParent parent) { _parent = parent; }

        [DisableDump]
        internal override Root RootContext { get { return _parent.RootContext; } }
        [Node]
        [DisableDump]
        public TParent Parent { get { return _parent; } }

        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return _parent; } }
        Result IConverter.Result(Category category) { return ParentConversionResult(category); }
        protected abstract Result ParentConversionResult(Category category);
    }
}