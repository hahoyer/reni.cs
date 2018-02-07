using System;
using Bnf.Contexts;
using Bnf.Features;
using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        protected readonly Syntax Parent;
        readonly FunctionCache<Context, Result> ResultCache;

        protected Form(Syntax parent)
        {
            Parent = parent;
            ResultCache = new FunctionCache<Context, Result>(GetResult);
        }

        Result IForm.GetResult(Context context) => ResultCache[context];

        protected abstract Result GetResult(Context context);
    }
}