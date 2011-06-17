using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Type;

namespace Reni.Context
{
    [Serializable]
    internal sealed class Function : Child
    {
        private readonly DictionaryEx<ContextBase, FunctionContextObject> _functionContexts;
        private readonly TypeBase _argsType;

        internal Function(TypeBase argsType)
        {
            _argsType = argsType;
            _functionContexts = new DictionaryEx<ContextBase, FunctionContextObject>(parent => new FunctionContextObject(this, parent));
        }

        internal TypeBase ArgsType { get { return _argsType; } }

        internal FunctionContextObject SpawnFunctionContext(ContextBase parent) { return _functionContexts.Find(parent); }
    }
}