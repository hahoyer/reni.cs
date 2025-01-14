using Reni.Feature;

namespace Reni.Type;

sealed class FeaturePathBridge<TProvider> : DumpableObject, ISearchTarget
    where TProvider : TypeBase
{
    [EnableDump]
    readonly TProvider InnerProvider;
    [EnableDump]
    readonly TypeBase MainProvider;

    public FeaturePathBridge(TProvider innerProvider, TypeBase mainProvider)
    {
        InnerProvider = innerProvider;
        MainProvider = mainProvider;
    }

}