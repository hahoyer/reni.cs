using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [AdditionalNodeInfo("NodeDump")]
    internal abstract class Base : ReniObject
    {
        private static int _nextObjectId;

        protected Base() : base(_nextObjectId++) {}

        /// <summary>
        /// true only for end token
        /// </summary>
        /// <returns></returns>
        [DumpData(false)]
        internal virtual bool IsEnd { get { return false; } }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal virtual Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        internal protected Syntax.Base CreateSpecialSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            return new Special(left, token, right, Feature);
        }

        protected abstract Feature Feature { get; }

        /// <summary>
        /// The name of the token for lookup in prio table of parser.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// created 31.03.2007 23:36 on SAPHIRE by HH
        internal virtual string PrioTableName(string name)
        {
            return name;
        }

        /// <summary>
        /// Results the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        internal virtual Result Result(Syntax.Base left, Token token, Syntax.Base right, Context.Base context,
            Category category)
        {
            NotImplementedMethod(left, token, right, context, category);
            return null;
        }
    }
}