using System;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    internal class ConversionParameter : ReniObject
    {
        private static ConversionParameter _instance;
        private readonly bool _isDisableCut;
        private readonly bool _isUseConverter;

        private ConversionParameter(bool isUseConverter, bool isDisableCut)
        {
            _isUseConverter = isUseConverter;
            _isDisableCut = isDisableCut;
        }

        [IsDumpEnabled(false)]
        internal ConversionParameter EnableCut { get { return new ConversionParameter(IsUseConverter, false); } }
        [IsDumpEnabled(false)]
        internal ConversionParameter DontUseConverter { get { return new ConversionParameter(false, IsDisableCut); } }

        internal bool IsDisableCut { get { return _isDisableCut; } }
        internal bool IsUseConverter { get { return _isUseConverter; } }

        internal static ConversionParameter Instance
        {
            get { return _instance ?? (_instance = new ConversionParameter(true, true)); }
        }
    }
}