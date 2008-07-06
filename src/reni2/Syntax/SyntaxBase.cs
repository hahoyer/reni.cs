using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;

namespace Reni.Syntax
{
    internal interface ICompileSyntax
    {
        Result Result(ContextBase context, Category category);
        string DumpShort();
        string FilePosition();
    }

    /// <summary>
    /// Syntax tree is construced by objects of this type. 
    /// The center is a token, and it can have a left and a right leave (of type syntax).
    /// Furthermore it contains if it has been created by match of two tokens
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class SyntaxBase : ReniObject
    {
        internal virtual string DumpShort()
        {
            NotImplementedMethod();
            return "";
        }

        /// <summary>
        /// What to when syntax element is surrounded by parenthesis?
        /// </summary>
        /// <returns></returns>
        /// created 19.07.2007 23:20 on HAHOYER-DELL by hh
        public virtual SyntaxBase SurroundedByParenthesis()
        {
            return this;
        }
    }
}