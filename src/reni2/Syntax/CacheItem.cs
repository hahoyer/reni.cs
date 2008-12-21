using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Syntax
{
    internal interface IResultCacheItem
    {
        Result Result(Category category);
    }

    [Serializable]
    internal sealed class CacheItem : ReniObject, IIconKeyProvider, IResultCacheItem
    {
        private readonly ICompileSyntax _syntax;
        private readonly ContextBase _context;
        private readonly Result _data = new Result();

        [Node]
        public Result Data { get { return _data; } }

        public CacheItem(ICompileSyntax syntax, ContextBase context)
        {
            if(syntax == null)
                throw new NullReferenceException("parameter \"syntax\" must not be null");
            _syntax = syntax;
            _context = context;
        }

        [DebuggerHidden]
        Result IResultCacheItem.Result(Category category) { return _data.AddCategories(category, _context, _syntax); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Cache"; } }
    }
}