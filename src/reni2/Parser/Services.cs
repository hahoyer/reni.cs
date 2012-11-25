#region Copyright (C) 2012

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

#endregion

using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.ReniParser;

namespace Reni.Parser
{
    public static class Services
    {
        public static PrioTable FormatPrioTable(string text) { return PrioTable.FromText(text); }

        public static Image SyntaxGraph(PrioTable prioTable, string code)
        {
            if(code == null)
                return new Bitmap(1, 1);

            var parser = new ParserInst(new ReniScanner(), new MainTokenFactory(prioTable));
            var syntax = parser.Compile(new Source(code));
            return SyntaxDrawer.DrawBitmap(syntax);
        }

        public static string ToBase64(this Image image)
        {
            var ic = new ImageConverter();
            Tracer.Assert(image != null, () => "image != null");
            var convertTo = (byte[]) ic.ConvertTo(image, typeof(byte[]));
            Tracer.Assert(convertTo != null, () => "convertTo != null");
            return Convert.ToBase64String(convertTo, Base64FormattingOptions.InsertLineBreaks);
        }

    }
}