using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Syntax
{
    internal abstract class InternalResultProvider : ReniObject, IInternalResultProvider
    {
        IInternalResultProvider IInternalResultProvider.CreateSequence(IInternalResultProvider secondElement)
        {
            return secondElement.CreateReverseSequence(this);
        }

        IInternalResultProvider IInternalResultProvider.CreateReverseSequence(IInternalResultProvider firstElement)
        {
            return new ResultProviderPair(firstElement,this);
        }

        Result IResultProvider.Result(Category category) { return Result(category); }
        protected abstract Result Result(Category category);
    }

    /// <summary>
    /// For each syntax  object, the environment is mapped against the corresponding compilation result.
    /// The mapping for one environment is extended, each time more categories are requested
    /// </summary>
    [Serializable]
    internal sealed class CacheItem : InternalResultProvider, IIconKeyProvider
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

        //[DebuggerHidden]
        protected override Result Result(Category category) { return _data.AddCategories(category, _context, _syntax); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Cache"; } }
    }

    internal class ResultProviderPair : InternalResultProvider
    {
        [Node]
        internal readonly IInternalResultProvider FirstElement;
        [Node]
        internal readonly IInternalResultProvider SecondElement;

        public ResultProviderPair(IInternalResultProvider firstElement, IInternalResultProvider secondElement)
        {
            FirstElement = firstElement;
            SecondElement = secondElement;
        }

        protected override Result Result(Category category)
        {
            return FirstElement.Result(category).CreateSequence(SecondElement.Result(category));
        }
    }
}