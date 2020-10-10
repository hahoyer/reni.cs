using hw.DebugFormatter;

namespace Reni.Helper
{
    interface ITree<out TTarget>
    {
        [DisableDump]
        int LeftDirectChildCount { get; }

        [DisableDump]
        int DirectChildCount { get; }

        TTarget GetDirectChild(int index);
    }
}