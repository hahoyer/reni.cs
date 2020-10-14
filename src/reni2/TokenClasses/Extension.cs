using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    static class Extension
    {
        internal static BinaryTree[] Combine(this IEnumerable<IEnumerable<BinaryTree>> targets)
        {
            var target = targets.Concat().ToArray();
            return target
                .Where(b => !target.Any(p => p != b && p.SourcePart.Contains(b.SourcePart)))
                .ToArray();
        }
    }
}