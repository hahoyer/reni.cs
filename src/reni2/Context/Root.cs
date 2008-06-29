using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Context
{
    /// <summary>
    /// Root environment of compilation process
    /// </summary>
    internal sealed class Root : ContextBase
    {
        private readonly FunctionList _function = new FunctionList();

        /// <summary>
        /// Parameter to describe alignment for references
        /// </summary>
        public override RefAlignParam RefAlignParam { get { return DefaultRefAlignParam; } }

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(3, Reni.Size.Create(32)); } }

        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        public override Root RootContext { get { return this; } }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return SearchResult<IContextFeature>.Failure(this, defineable);
        }

        /// <summary>
        /// Compiles the functions.
        /// </summary>
        /// <returns></returns>
        /// [created 13.05.2006 18:51]
        internal List<Container> CompileFunctions()
        {
            return _function.Compile();
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <value>The function.</value>
        /// created 03.01.2007 20:23
        [DumpData(false)]
        public FunctionList Functions { get { return _function; } }

        /// <summary>
        /// Creates the function call.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="body">The body.</param>
        /// <param name="argsResult">The args result.</param>
        /// <returns></returns>
        /// created 06.11.2006 22:54
        public Result CreateFunctionCall(ContextBase context, Category category, ICompileSyntax body, Result argsResult)
        {
            return Functions.Find(body, context, argsResult.Type).CreateCall(category, argsResult);
        }
    }
}