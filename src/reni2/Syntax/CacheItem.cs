using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Type;

namespace Reni.Syntax
{
    internal abstract class InternalResultProvider : ReniObject, IInternalResultProvider
    {
        [DebuggerHidden]
        Result IResultProvider.Result(Category category) { return Result(category); }
        protected abstract Result Result(Category category);
    }

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

    sealed internal class ConversionResultProvider: InternalResultProvider
    {
        internal readonly ICompileSyntax Syntax;
        internal readonly ContextBase ContextBase;
        internal readonly TypeBase Target;

        public ConversionResultProvider(ICompileSyntax syntax, ContextBase contextBase, TypeBase target)
        {
            Syntax = syntax;
            ContextBase = contextBase;
            Target = target;
        }

        protected override Result Result(Category category)
        {
            return  ContextBase.Result(category | Category.Type, Syntax).ConvertTo(Target);
        }
    }

    sealed internal class ResultProvider : InternalResultProvider
    {
        [Node]
        internal readonly ICompileSyntax Syntax;
        [Node]
        internal readonly ContextBase ContextBase;
        
        public ResultProvider(ICompileSyntax syntax, ContextBase contextBase)
        {
            Syntax = syntax;
            ContextBase = contextBase;
        }

        protected override Result Result(Category category)
        {
            return ContextBase.Result(category | Category.Type, Syntax);
        }
    }
}