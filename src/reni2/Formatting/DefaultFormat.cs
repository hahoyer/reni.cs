using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class DefaultFormat : DumpableObject, IConfiguration
    {
        public static readonly DefaultFormat Instance = new DefaultFormat();
        DefaultFormat() { }
        ISubConfiguration IConfiguration.Assess(BinaryTree target) => DefaultSubFormat.Instance;
    }
}