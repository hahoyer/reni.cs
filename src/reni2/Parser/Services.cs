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

namespace Reni.Parser
{
    public static class Services
    {
        public static PrioTable FormatPrioTable(string text) { return PrioTable.FromText(text); }
        
        public static Image SyntaxGraph(PrioTable prioTable, string text)
        {
            var frame = new Rectangle(0, 0, 800, 600);
            var bitmap = new Bitmap(frame.Size.Width, frame.Size.Height);
            var graphics = Graphics.FromImage(bitmap);
            var font = new Font("Arial", 16);
            var brush = new SolidBrush(Color.Black);
            graphics.DrawString(text, font, brush, 400, 300);
            return bitmap;
        }
    }
}