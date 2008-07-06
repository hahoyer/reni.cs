using Reni.Context;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : Child
    {
        internal readonly Context Context;
        internal readonly int Position;

        public ContextAtPosition(Context context, int position): base(context)
        {
            Context = context;
            Position = position;
        }
    }
}