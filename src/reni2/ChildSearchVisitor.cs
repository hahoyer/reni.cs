using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni
{
    internal class ChildSearchVisitor<TFeature, TType> : SearchVisitor<IConverter<TFeature, TType>>
        where TFeature : class
        where TType : IDumpShortProvider
    {
        private readonly SearchVisitor<TFeature> _parent;
        private readonly TType _target;

        public ChildSearchVisitor(SearchVisitor<TFeature> parent, TType target)
        {
            _parent = parent;
            _target = target;
        }

        internal override bool IsSuccessFull { get { return _parent.IsSuccessFull; } }

        internal override IConverter<TFeature, TType> InternalResult
        {
            set
            {
                if(value == null)
                    return;
                Tracer.Assert(!IsSuccessFull);
                _parent.InternalResult = value.Convert(_target);
            }
        }

        internal override Defineable Defineable { get { return _parent.Defineable; } }
    }
}