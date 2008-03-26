using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Parser;

namespace Reni.Context
{
    /// <summary>
    /// Root environment of compilation process
    /// </summary>
    internal sealed class Root : Base
    {
        readonly FunctionList _function = new FunctionList();

        /// <summary>
        /// Parameter to describe alignment for references
        /// </summary>
        public override RefAlignParam RefAlignParam{get { return DefaultRefAlignParam; }}

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(3, Size.Create(32)); } }

        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        public override Root RootContext { get { return this; } }

        internal override ContextSearchResult SearchDefineable(DefineableToken defineableToken)
        {
            return null;
        }

        internal override bool IsChildOf(Base context)
        {
            return false;
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
        public FunctionList Functions
        {
            get { return _function; }
        }

        /// <summary>
        /// Creates the function call.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="body">The body.</param>
        /// <param name="argsResult">The args result.</param>
        /// <returns></returns>
        /// created 06.11.2006 22:54
        public Result CreateFunctionCall(Base context, Category category, Syntax.Base body, Result argsResult)
        {
            %return Functions.Find(body, context, argsResult.Type).CreateCall(category, argsResult);
        }

    }
}

