namespace Reni.Type
{
    internal sealed  class Void : TypeBase
    {
        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override bool IsVoid { get { return true; } }
        internal override string DumpPrintText { get { return "void"; } }

        public override TypeBase CreatePair(TypeBase second)
        {
            return second;
        }

        protected override TypeBase CreateReversePair(TypeBase first)
        {
            return first;
        }

        internal override Result DumpPrint(Category category)
        {
            return CreateResult(category);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return false;
        }

        internal new static Result CreateResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        internal new static Result CreateResult(Category category, Result.GetCode getCode)
        {
            return CreateVoid.CreateResult(category, getCode);
        }

        internal new static Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs)
        {
            return CreateVoid.CreateResult(category, getCode, getRefs);
        }

        internal override string DumpShort()
        {
            return "void";
        }
    }
}