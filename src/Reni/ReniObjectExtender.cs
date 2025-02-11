using System.Reflection;
using hw.Parser;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni;

static class Extension
{
    sealed class EqualityComparer<T>
        : IEqualityComparer<T>
    {
        new readonly Func<T?, T?, bool> Equals;

        public EqualityComparer(Func<T?, T?, bool> equals) => Equals = equals;

        bool IEqualityComparer<T>.Equals(T? x, T? y) => Equals(x, y);
        int IEqualityComparer<T>.GetHashCode(T obj) => 1;
    }

    // will throw an exception if not a ReniObject
    internal static int GetObjectId(this object reniObject)
        => ((DumpableObject)reniObject).ObjectId;

    // will throw an exception if not a ReniObject
    [PublicAPI]
    internal static int? GetObjectId<T>(this object reniObject)
    {
        if(reniObject is T)
            return ((DumpableObject)reniObject).ObjectId;

        return null;
    }

    internal static string NodeDump(this object o)
        => o is DumpableObject r? r.NodeDump : o.ToString()!;

    internal static bool IsBelongingTo(this IBelongingsMatcher current, ITokenClass? other)
        => other is IBelongingsMatcher otherMatcher && current.IsBelongingTo(otherMatcher);

    internal static bool IsBelongingTo(this ITokenClass current, ITokenClass? other)
        => (current as IBelongingsMatcher)?.IsBelongingTo(other) ?? false;


    internal static IEnumerable<Syntax> CheckedItemsAsLongAs(this Syntax? target, Func<Syntax, bool> condition)
    {
        if(target == null || !condition(target))
            yield break;

        foreach(var result in target.ItemsAsLongAs(condition))
            yield return result;
    }

    [PublicAPI]
    internal static IEnumerable<System.Type> DerivedClasses<T>()
        => TypeNameExtender.Types.Where(item => item.Is<T>());

    internal static IEnumerable<string> GetTokenIds<T>()
        => DerivedClasses<T>()
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
                    .DeclaringType!
                    .GetAttributes<VariantAttribute>(false)
                    .Select
                        (item => (string)((MethodInfo)member).Invoke(null, item.CreationParameter)!);
            case MemberTypes.Field:
                return [(string)((FieldInfo)member).GetValue(null)!];
        }

        Dumpable.NotImplementedFunction(member.Name);
        return null!;
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

    public static IEqualityComparer<T> Comparer<T>(Func<T?, T?, bool> equals)
        => new EqualityComparer<T>(equals);
}