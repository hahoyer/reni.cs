using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Type : TypeBase
    {
        private readonly Container _container;
        private readonly ContextBase _context;
        private readonly int _currentCompilePosition;

        public Type(ContextBase context, Container struc, int currentCompilePosition)
        {
            _context = context;
            _container = struc;
            _currentCompilePosition = currentCompilePosition;
        }

        [Node]
        public int CurrentCompilePosition { get { return _currentCompilePosition; } }

        [Node]
        public Container Container { get { return _container; } }

        [Node]
        public ContextBase Context { get { return _context; } }

        public override Size Size { get { return _container.VisitSize(_context, _currentCompilePosition); } }

        [DumpData(false)]
        internal override string DumpPrintText { get { return "#(#context " + _context.ObjectId + "#)# (" + _container.DumpPrintText(_context) + ")"; } }

        internal override bool IsPending { get { return _container.IsPendingType(_context); } }

        internal override Result MoveHandler(Category category)
        {
            return _container.MoveHandler(category, _context, _currentCompilePosition);
        }

        internal override bool HasConverterTo(TypeBase dest)
        {
            return _container.HasConverterTo(_context, dest);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            var voidDest = dest as Void;
            if(voidDest != null)
                return _container.IsConvertableToVoid(_context);
            return base.IsConvertableToVirt(dest, conversionFeature);
        }

        internal override CodeBase CreateRefCodeForContext(ContextBase context)
        {
            return context.CreateRefForStruct(this);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return _container.ConvertTo(category, _context, dest);
        }

        internal override Result DestructorHandler(Category category)
        {
            return _container.DestructorHandler
                (
                _context,
                category,
                CreateRef(_context.RefAlignParam).CreateArgResult(category),
                _currentCompilePosition
                );
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            return _container.DumpPrintFromRef(category, _context, refAlignParam);
        }

        public Result AccessFromArg(Category category, int position)
        {
            var result = _container.VisitElementFromContextRef(_context, category, position);
            var containerContext = _context.CreateStructContext(_container);
            var argsRef = CodeBase
                .CreateArg(_context.RefAlignParam.RefSize)
                .CreateRefPlus(_context.RefAlignParam, Size);
            return result.ReplaceRelativeContextRef(containerContext, argsRef);
        }
    }
}