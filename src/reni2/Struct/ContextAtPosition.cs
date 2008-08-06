using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class ContextAtPosition : StructContextBase
    {
        internal readonly int _position;
        private readonly FullContext _context;

        internal ContextAtPosition(FullContext context, int position)
            : base(context.Parent, context.Container)
        {
            _position = position;
            _context = context;
        }

        [DumpData(false)]
        public override IContextRefInCode ForCode { get { return _context; } }
        [Node]
        internal override int Position { get { return _position; } }
        [DumpData(false)]
        internal override FullContext Context { get { return _context; } }
        internal override string DumpShort() { return base.DumpShort() + "@" + Position; }
        internal override IStructContext FindStruct() { return this; }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            var containerResult = Container.SearchFromStructContextAtPosition(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature,
                this);
            if(result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }
    }
}