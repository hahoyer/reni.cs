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

        internal readonly Tuple<SourceSyntax, bool>[] Data;

        Rulers() { Data = new Tuple<SourceSyntax, bool>[] {}; }

        Rulers(IEnumerable<Tuple<SourceSyntax, bool>> data, SourceSyntax other, bool isMultiline)
        {
            Data = data
                .Concat(new[] {new Tuple<SourceSyntax, bool>(other, isMultiline)})
                .ToArray();
        }

        public bool HasMultiLine => Data.Any(item => item.Item2);
        public bool HasSingleLine => Data.Any(item => !item.Item2);
        public bool LastIsMultiLine => Data.Last()?.Item2 ?? false;

        public Rulers Concat(SourceSyntax other, bool isMultiline)
            => new Rulers(Data, other, isMultiline);

        public bool IsMultiLine(SourceSyntax lineBreakRuler)
            => Data.FirstOrDefault(item => item.Item1 == lineBreakRuler)?.Item2 ?? false;
    }
}