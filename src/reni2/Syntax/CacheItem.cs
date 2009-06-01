using HWClassLibrary.TreeStructure;
using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Syntax
{
    [Serializable]
    internal sealed class CacheItem : ReniObject, IIconKeyProvider
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
        public Result Result(Category category)
        {
            _data.AddCategories(_context, category, _syntax);
            Tracer.Assert(category <= Data.CompleteCategory);
            return Data & category;
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Cache"; } }
    }
}