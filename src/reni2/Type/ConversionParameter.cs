// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    sealed class ConversionParameter : ReniObject
    {
        static ConversionParameter _instance;
        static ConversionParameter _instanceForSearchConversiuon;
        readonly bool _isDisableCut;
        readonly bool _isUseConverter;

        ConversionParameter(bool isUseConverter, bool isDisableCut)
        {
            _isUseConverter = isUseConverter;
            _isDisableCut = isDisableCut;
        }

        [DisableDump]
        internal ConversionParameter EnableCut { get { return new ConversionParameter(IsUseConverter, false); } }

        [DisableDump]
        internal ConversionParameter DontUseConverter { get { return new ConversionParameter(false, IsDisableCut); } }

        internal bool IsDisableCut { get { return _isDisableCut; } }
        internal bool IsUseConverter { get { return _isUseConverter; } }

        internal static ConversionParameter Instance { get { return _instance ?? (_instance = new ConversionParameter(true, true)); } }
        internal static ConversionParameter InstanceForSearchConversion { get { return _instanceForSearchConversiuon ?? (_instanceForSearchConversiuon = new ConversionParameter(false, true)); } }
    }
}