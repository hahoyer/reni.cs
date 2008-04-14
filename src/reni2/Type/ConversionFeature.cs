namespace Reni.Type
{
    internal class ConversionFeature : ReniObject
    {
        private static ConversionFeature _instance;
        private readonly bool _isDisableCut;
        private readonly bool _isUseConverter;

        private ConversionFeature(bool isUseConverter, bool isDisableCut)
        {
            _isUseConverter = isUseConverter;
            _isDisableCut = isDisableCut;
        }

        internal ConversionFeature EnableCut { get { return new ConversionFeature(IsUseConverter, false); } }
        internal ConversionFeature DontUseConverter { get { return new ConversionFeature(false, IsDisableCut); } }

        internal bool IsDisableCut { get { return _isDisableCut; } }
        internal bool IsUseConverter { get { return _isUseConverter; } }

        internal static ConversionFeature Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new ConversionFeature(true, true);
                return _instance;
            }
        }
    }
}