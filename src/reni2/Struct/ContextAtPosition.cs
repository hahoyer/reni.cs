using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : Child
    {
        [DumpData(false)]
        internal readonly Context Context;
        [DumpData(false)]
        internal readonly int Position;

        public ContextAtPosition(Context context, int position): base(context)
        {
            Context = context;
            Position = position;
        }
    }
}