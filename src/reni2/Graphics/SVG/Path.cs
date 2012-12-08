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
using System.Xml.Serialization;
using HWClassLibrary.Debug;

namespace Reni.Graphics.SVG
{
    public sealed class Path : Content
    {
        [XmlAttribute("d")]
        public string PathData;

        internal sealed class Arc : Element
        {
            [EnableDump]
            readonly Size _radii;
            [EnableDump]
            readonly Size _end;
            [EnableDump]
            readonly int _xAxisRotation;
            [EnableDump]
            readonly bool _largeArcFlag;
            [EnableDump]
            readonly bool _sweepFlag;

            public Arc(Size radii, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
                : base(true)
            {
                _radii = radii;
                _end = end;
                _xAxisRotation = xAxisRotation;
                _largeArcFlag = largeArcFlag;
                _sweepFlag = sweepFlag;
            }

            public Arc(int radius, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
                : this(new Size(radius, radius), end, largeArcFlag, sweepFlag, xAxisRotation) { }

            internal override Size Size { get { return _end; } }
            internal override string FormatElement
            {
                get
                {
                    return
                        "a"
                        + FormatPair(_radii)
                        + " "
                        + _xAxisRotation
                        + " "
                        + (_largeArcFlag ? 1 : 0)
                        + " "
                        + (_sweepFlag ? 1 : 0)
                        + " "
                        + FormatSize;
                }
            }
        }

        sealed class Home : Element
        {
            public Home()
                : base(false) { }
            internal override Size Size { get { return new Size(0, 0); } }

            internal override string FormatElement { get { return "M" + FormatSize; } }
        }

        internal sealed class Line : Element
        {
            [EnableDump]
            readonly Size _end;
            public Line(Size end, bool isVisible = true)
                : base(isVisible) { _end = end; }

            internal override Size Size { get { return _end; } }

            internal override string FormatElement { get { return (IsVisible ? "l" : "m") + FormatSize; } }
        }

        internal sealed class HorizontalLine : Element
        {
            [EnableDump]
            readonly int _length;

            public HorizontalLine(int length, bool isVisible = true)
                : base(isVisible) { _length = length; }

            internal override Size Size { get { return new Size(_length, 0); } }
            internal override string FormatElement { get { return IsVisible ? "h" + _length : "m" + FormatSize; } }
        }

        sealed class VerticalLine : Element
        {
            [EnableDump]
            readonly int _length;

            public VerticalLine(int length, bool isVisible = true)
                : base(isVisible) { _length = length; }

            internal override Size Size { get { return new Size(0, _length); } }
            internal override string FormatElement { get { return IsVisible ? "v" + _length : "m" + FormatSize; } }
        }

        internal abstract class Element : ReniObject
        {
            protected readonly bool IsVisible;

            protected Element(bool isVisible) { IsVisible = isVisible; }

            internal abstract Size Size { get; }
            internal abstract string FormatElement { get; }
            internal string FormatSize { get { return FormatPair(Size); } }
            protected static string FormatPair(Size size) { return size.Width + "," + size.Height; }
        }

        internal static string Format(params Element[] path)
        {
            var current = new Point(0, 0);
            var result = "M0,0";
            foreach(var t in path)
            {
                result += " ";
                result += t.FormatElement;
                current += t.Size;
            }

            return result;
        }

        internal static string CloseAndFormat(params Element[] path) { return Format(path) + " Z"; }
    }
}