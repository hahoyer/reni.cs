using hw.DebugFormatter;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        static int NextObjectId;

        protected readonly Syntax Parent;

        protected Form(Syntax parent)
            : base(NextObjectId++) => Parent = parent;
    }
}