using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Base clas for compiler tokens
    /// </summary>
    [AdditionalNodeInfo("NodeDump")]
    internal abstract class Base : ReniObject
    {
        private static int _nextObjectId = 0;
        public Base(): base(_nextObjectId++){}
        /// <summary>
        /// true only for end token
        /// </summary>
        /// <returns></returns>
        [DumpData(false)]
        public virtual bool IsEnd { get { return false; } }

        /// <summary>
        /// Assembles TokenClass into context
        /// </summary>
        /// <param name="e">compilation context</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        internal virtual Context.Base Context(Context.Base e, Syntax.Base s)
        {
            NotImplementedMethod(e,s);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a list 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="x"></param>
        public virtual void ToList(List<Syntax.Base> result, Syntax.Base x)
        {
            result.Add(x);
        }

        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        virtual public Syntax.Base CreateSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected static Syntax.Base CreateSpecialSyntax(Syntax.Base left, Token token, Syntax.Base right)
        {
            return new Syntax.Special(left, token, right);
        }

        /// <summary>
        /// The name of the token for lookup in prio table of parser.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// created 31.03.2007 23:36 on SAPHIRE by HH
        public virtual string PrioTableName(string name)
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
        virtual internal Result Result(Syntax.Base left, Token token, Syntax.Base right, Context.Base context, Category category)
        {
            NotImplementedMethod(left, token, right, context, category);
            return null;
        }

    }

}