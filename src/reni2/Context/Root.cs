using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Context
{
    /// <summary>
    /// Root environment of compilation process
    /// </summary>
    public sealed class Root : Base
    {
        FunctionList _function = new FunctionList();
        Type.Base _type;

        public Root()
        {
            _type = new Type.Root(this).CreateRef(DefaultRefAlignParam);
        }

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
            return Functions.Find(body, context, argsResult.Type).CreateCall(category, argsResult);
        }

    }
}

namespace Reni.Type
{
    internal class Root: Base
    {
        private readonly Context.Root _root;

        public Root(Context.Root root)
        {
            _root = root;
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        public override string DumpPrintText { get { return "#(#root#)#"; } }
    }
}
