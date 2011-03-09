using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.TokenClasses;

namespace Reni
{
    internal class RootSearchVisitor<TFeature> : SearchVisitor<TFeature>
        where TFeature : class
    {
        private readonly Defineable _defineable;
        private TFeature _result;

        internal RootSearchVisitor(Defineable defineable) { _defineable = defineable; }

        internal override bool IsSuccessFull { get { return _result != null; } }

        internal override TFeature InternalResult
        {
            set
            {
                Tracer.Assert(_result == null || _result == value);
                _result = value;
            }
        }

        internal TFeature Result { get { return _result; } }

        internal override Defineable Defineable { get { return _defineable; } }
    }
}