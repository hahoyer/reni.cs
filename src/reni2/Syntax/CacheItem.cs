using System;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Syntax
{
    /// <summary>
    /// For each syntax  object, the environment is mapped against the corresponding compilation result.
    /// The mapping for one environment is extended, each time more categories are requested
    /// </summary>
    internal sealed class CacheItem : ReniObject
    {
        private readonly Base _syntax;
        private readonly Context.Base _context;
        private Result _data = new Result();

        [Node]
        public Result Data { get { return _data; } }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="environment"></param>
        public CacheItem(Base syntax, Context.Base environment)
        {
            if (syntax == null)
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
        public Result Visit(Category category)
        {
            return _data.Visit(category, _context, _syntax);
        }
    }
}