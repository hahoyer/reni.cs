using hw.DebugFormatter;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        static int NextObjectId;

        protected readonly Syntax Parent;

        protected Form(Syntax parent)
            : base(NextObjectId++) => Parent = parent;

        protected Form(Syntax parent, int objectId)
            : base(objectId) => Parent = parent;
    }
}