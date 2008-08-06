using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Pending : TypeBase, IIconKeyProvider
    {
        protected override Size GetSize() { return Size.Pending; }

        internal override string DumpPrintText { get { return "#(# Prendig type #)#"; } }
        internal override bool IsPending { get { return true; } }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return dest.CreateResult
                (
                category,
                () => CodeBase.Pending,
                () => Refs.Pending
                );
        }

        internal override bool IsConvertableToVirt(TypeBase dest,
            ConversionFeature conversionFeature) { return true; }

        internal override string DumpShort() { return "pending"; }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Pending"; } }
    }
}