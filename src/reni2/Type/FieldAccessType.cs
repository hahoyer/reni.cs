using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;


namespace Reni.Type
{
    sealed class FieldAccessType : DataSetterTargetType
    {
        [EnableDump]
        readonly CompoundView _compoundView;
        [EnableDump]
        readonly int _position;

        internal FieldAccessType(CompoundView compoundView, int position)
        {
            _compoundView = compoundView;
            _position = position;
        }

        public override bool IsReassignPossible => _compoundView.Compound.Syntax.IsReassignable(_position);

        internal override TypeBase ValueType { get { return _compoundView.ValueType(_position); } }

        [DisableDump]
        internal override bool Hllw { get { return false; } }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "{" + _compoundView.NodeDump + "@" + _position + "}";
        }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        Size FieldOffset { get { return _compoundView.FieldOffset(_position); } }

        protected override CodeBase GetterCode() { return ArgCode.ReferencePlus(FieldOffset); }

        protected override CodeBase SetterCode()
        {
            return
                Pair(ValueType.PointerKind)
                    .ArgCode
                    .Assignment(ValueType.Size);
        }

        internal override Result DestinationResult(Category category)
        {
            return base
                .DestinationResult(category)
                .AddToReference(() => FieldOffset);
        }

        internal override int? SmartSequenceLength(TypeBase elementType)
        {
            return ValueType.SmartSequenceLength(elementType);
        }

        internal override int? SmartArrayLength(TypeBase elementType)
        {
            return ValueType.SmartArrayLength(elementType);
        }
    }
}