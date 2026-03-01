using System.Collections;
using Reni.DeclarationOptions;

namespace ReniUI;

static class Extension
{
    extension(Declaration[] declarations)
    {
        internal Declaration[] Filter(string searchId)
            => declarations.Where(declaration => declaration.IsMatch(searchId)).ToArray();
    }

    sealed class QueryClass<T> : IEnumerable<T>
    {
        readonly Func<IEnumerable<T>> Function;
        public QueryClass(Func<IEnumerable<T>> function) => Function = function;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator<T> GetEnumerator() => Function().GetEnumerator();
    }

    internal static IEnumerable<T> Query<T>(Func<IEnumerable<T>> function)
        => new QueryClass<T>(function);
}
