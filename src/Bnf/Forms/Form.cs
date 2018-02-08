using System;
using Bnf.Contexts;
using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        protected readonly Syntax Parent;
        readonly FunctionCache<IContext, string> ResultCache;

        protected Form(Syntax parent)
        {
            Parent = parent;
            ResultCache = new FunctionCache<IContext, string>(GetResult);
        }

        string IForm.GetResult(IContext context) => ResultCache[context];

        protected abstract string GetResult(IContext context);
    }
}