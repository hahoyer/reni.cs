using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    internal class CompileSyntax : ParsedSyntax, ICompileSyntax
    {
        internal CompileSyntax(Token token)
            : base(token) {}

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            var trace = ObjectId < 0;
            StartMethodDump(trace, context,category);
            if(category.HasInternal || !(category.HasCode || category.HasRefs))
                return ReturnMethodDump(trace, Result(context, category));
            var result = Result(context, category | Category.Internal | Category.Type);
            Dump(trace, "result", result);
            return ReturnMethodDump(trace, result.CreateStatement(category, result.Internal));
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal protected override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return this;
        }

        [DumpData(false)]
        internal protected override ICompileSyntax ToCompileSyntax { get { return this; } }

        protected internal override IParsedSyntax CreateSyntax(Token token,IParsedSyntax right)
        {
            return new ExpressionSyntax(this, token, ToCompiledSyntaxOrNull(right));
        }

    }
}