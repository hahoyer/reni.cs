using hw.DebugFormatter;
using Reni.Feature;

namespace Reni.Type
{
    sealed class FeaturePathBridge<TProvider> : DumpableObject, ISearchTarget
        where TProvider : TypeBase
    {
        [EnableDump]
        readonly TProvider _innerProvider;
        [EnableDump]
        readonly TypeBase _mainProvider;

        public FeaturePathBridge(TProvider innerProvider, TypeBase mainProvider)
        {
            _innerProvider = innerProvider;
            _mainProvider = mainProvider;
        }

    }
}