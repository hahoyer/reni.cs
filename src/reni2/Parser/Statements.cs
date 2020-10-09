using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.Parser
{
    [Obsolete("", true)]
    sealed class Statements : DumpableObject
    {
        public Statements(List type, IEnumerable<Statement> data)
        {
            Type = type;
            Data = data.ToArray();
            StopByObjectIds();
        }

        [EnableDump]
        List Type { get; }
        [EnableDump]
        Statement[] Data { get; }
      }
}