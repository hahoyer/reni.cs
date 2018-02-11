using hw.DebugFormatter;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        protected readonly Syntax Parent;

        protected Form(Syntax parent) => Parent = parent;
    }
}