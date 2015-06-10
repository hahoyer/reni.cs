using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Rulers : DumpableObject
    {
        internal static readonly Rulers Empty = new Rulers();

        internal readonly Dictionary<SourceSyntax, bool> Data;

        Rulers() { Data = new Dictionary<SourceSyntax, bool>(); }

        Rulers(Dictionary<SourceSyntax, bool> data, SourceSyntax other, bool isMultiline)
        {
            Data = data.ToDictionary(item => item.Key, item => item.Value);
            Data[other] = isMultiline;
        }

        public Rulers Concat(SourceSyntax other, bool isMultiline)
            => new Rulers(Data, other, isMultiline);

        public bool IsMultiLine(SourceSyntax lineBreakRuler)
        {
            bool result;
            return Data.TryGetValue(lineBreakRuler, out result) && result;
        }
    }
}