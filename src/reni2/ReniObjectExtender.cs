using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni
{
    static class Extension
    {
        // will throw an exception if not a ReniObject
        internal static int GetObjectId(this object reniObject)
            => ((DumpableObject) reniObject).ObjectId;

        // will throw an exception if not a ReniObject
        internal static int? GetObjectId<T>(this object reniObject)
        {
            if(reniObject is T)
                return ((DumpableObject) reniObject).ObjectId;
            return null;
        }

        internal static string NodeDump(this object o)
        {
            var r = o as DumpableObject;
            if(r != null)
                return r.NodeDump;
            return o.ToString();
        }

        internal static bool IsBelongingTo(this IBelongingsMatcher current, ITokenClass other)
        {
            var otherMatcher = other as IBelongingsMatcher;
            return otherMatcher != null && current.IsBelongingTo(otherMatcher);
        }

        internal static bool IsBelongingTo(this ITokenClass current, ITokenClass other)
            => (current as IBelongingsMatcher)?.IsBelongingTo(other) ?? false;

        internal static IEnumerable<SourceSyntax> CheckedItemsAsLongAs
            (this SourceSyntax target, Func<SourceSyntax, bool> condition)
        {
            if(target == null || !condition(target))
                yield break;

            foreach(var result in target.ItemsAsLongAs(condition))
                yield return result;
        }
    }
}