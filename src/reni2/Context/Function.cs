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
        private readonly TypeBase _argsType;

        internal Function(TypeBase argsType)
        {
            _argsType = argsType;
        }

        internal TypeBase ArgsType { get { return _argsType; } }
    }
}