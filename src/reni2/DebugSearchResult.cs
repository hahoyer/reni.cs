using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;
using Reni.Type;

namespace Reni
{
    internal sealed class DebugSearchResult<TFeature> : ReniObject
        where TFeature : class
    {
        [UsedImplicitly]
        private readonly TypeBase _target;

        [UsedImplicitly]
        private readonly SearchVisitor<TFeature> _parent;

        [UsedImplicitly, EnableDump]
        private bool? _isFoundMode;

        public DebugSearchResult(SearchVisitor<TFeature> parent, TypeBase target = null)
        {
            _target = target;
            _parent = parent;
        }

        internal TypeBase Target { get { return _target; } }

        internal override string DumpShort()
        {
            var result = base.DumpShort();
            if(_isFoundMode != true)
                return result;

            if(_target == null)
                return result + " _isFoundMode";
            return result + " " + _target.DumpShort();
        }

        internal void SetSearchMode() { _isFoundMode = false; }
        internal void SetFoundMode() { _isFoundMode = true; }
    }
}