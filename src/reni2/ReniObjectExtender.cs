using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.Numeric;
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
            => o is DumpableObject r ? r.NodeDump : o.ToString();

        internal static bool IsBelongingTo(this IBelongingsMatcher current, ITokenClass other)
        {
            var otherMatcher = other as IBelongingsMatcher;
            return otherMatcher != null && current.IsBelongingTo(otherMatcher);
        }

        internal static bool IsBelongingTo(this ITokenClass current, ITokenClass other)
            => (current as IBelongingsMatcher)?.IsBelongingTo(other) ?? false;

        internal static IEnumerable<BinaryTree> CheckedItemsAsLongAs
            (this BinaryTree target, Func<BinaryTree, bool> condition)
        {
            if(target == null || !condition(target))
                yield break;

            foreach(var result in target.ItemsAsLongAs(condition))
                yield return result;
        }

        internal static IEnumerable<System.Type> DerivedClasses<T>()
            => TypeNameExtender.Types.Where(item => item.Is<T>());

        internal static IEnumerable<string> GetTokenIds<T>()
            => DerivedClasses<Operation>()
                .Where(item => item.IsSealed)
                .SelectMany(GetTokenIds);

        static IEnumerable<string> GetTokenIds(System.Type type)
            => type.GetMember("TokenId").SelectMany(GetNamesFromTokenIsMember);

        static IEnumerable<string> GetNamesFromTokenIsMember(MemberInfo member)
        {
            switch(member.MemberType)
            {
                case MemberTypes.Method:
                    return member
                        .DeclaringType
                        .GetAttributes<VariantAttribute>(false)
                        .Select
                            (item => (string) ((MethodInfo) member).Invoke(null, item.CreationParameter));
                case MemberTypes.Field: return new[] {(string) ((FieldInfo) member).GetValue(null)};
            }

            Dumpable.NotImplementedFunction(member.Name);
            return null;
        }

        internal static string GetFormattedNow()
        {
            var n = DateTime.Now;
            var result = "Date";
            result += n.Year.ToString("0000");
            result += n.Month.ToString("00");
            result += n.Day.ToString("00");
            result += "Time";
            result += n.Hour.ToString("00");
            result += n.Minute.ToString("00");
            result += n.Second.ToString("00");
            result += n.Millisecond.ToString("000");
            return result;
        }

        public static IEnumerable<T> SingleToArray<T>(this T target) {yield return target;}
    }
}