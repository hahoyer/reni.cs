using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Pending : TypeBase, IIconKeyProvider
    {
        internal override string DumpPrintText { get { return "#(# Prendig type #)#"; } }

        internal override bool IsConvertableToVirt(TypeBase dest,
                                                   ConversionFeature conversionFeature) { return true; }

        public override bool IsPending { get { return true; } }

        protected override Size GetSize()
        {
            return null;
        }

        internal override string DumpShort() { return "pending"; }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Pending"; } }
    }
}