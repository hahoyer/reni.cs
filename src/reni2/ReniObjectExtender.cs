// 
//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;

namespace Reni
{
    static class ReniObjectExtension
    {
        [DebuggerHidden]
        public static void StopByObjectId(this object t, int objectId)
        {
            var reniObject = t as DumpableObject;
            if(reniObject == null)
                return;
            reniObject.StopByObjectId(1, objectId);
        }

        // will throw an exception if not a ReniObject
        internal static int GetObjectId(this object reniObject) => ((DumpableObject) reniObject).ObjectId;
        // will throw an exception if not a ReniObject
        internal static int? GetObjectId<T>(this object reniObject)
        {
            if(reniObject is T)
                return ((DumpableObject)reniObject).ObjectId;
            return null;
        }
        internal static string NodeDump(this object o)
        {
            var r = o as DumpableObject;
            if(r != null)
                return r.NodeDump;
            return o.ToString();
        }
    }
}