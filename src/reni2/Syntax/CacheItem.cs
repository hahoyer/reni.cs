using System;
using System.Diagnostics;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;

namespace Reni.Syntax
{
    /// <summary>
    /// For each syntax  object, the environment is mapped against the corresponding compilation result.
    /// The mapping for one environment is extended, each time more categories are requested
    /// </summary>
    internal sealed class CacheItem : ReniObject
    {
        private readonly ICompileSyntax _syntax;
        private readonly ContextBase _context;
        private readonly Result _data = new Result();

        [Node]
        public Result Data { get { return _data; } }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="environment"></param>
        public CacheItem(ICompileSyntax syntax, ContextBase environment)
        {
            if(syntax == null)
                throw new NullReferenceException("parameter \"syntax\" must not be null");
            _syntax = syntax;
            _context = environment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        //[DebuggerHidden]
        public Result Result(Category category)
        {
            return _data.AddCategories(category, _context, _syntax);
        }
    }
}