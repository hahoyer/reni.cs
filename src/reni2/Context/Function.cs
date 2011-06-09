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
        private readonly DictionaryEx<ContextBase, FunctionContext> _functionContexts;
        private readonly TypeBase _argsType;

        internal Function(TypeBase argsType)
        {
            _argsType = argsType;
            _functionContexts = new DictionaryEx<ContextBase, FunctionContext>(parent => new FunctionContext(this, parent));
        }

        internal TypeBase ArgsType { get { return _argsType; } }

        internal FunctionContext SpawnFunctionContext(ContextBase parent) { return _functionContexts.Find(parent); }
    }
}