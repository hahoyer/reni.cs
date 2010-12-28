namespace Reni.Code
{
    internal sealed class BitsStackData: NonListStackData
    {
        private readonly BitsConst _data;
        public BitsStackData(BitsConst data) { _data = data; }
        internal BitsConst Data { get { return _data; } }
        internal override Size Size { get { return Data.Size; } }
    }
}