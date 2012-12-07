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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HWClassLibrary.Debug;

namespace Reni.Graphics.SVG
{
    [XmlRoot("div")]
    public sealed class Root
    {
        public string SerializeObject()
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };
            var writer = XmlWriter.Create(sb, settings);
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            new XmlSerializer(typeof(Root)).Serialize(writer, this, namespaces);
            writer.Close();
            var result = sb.ToString();
            return result;
        }

        public static Root CreateFromXML(string xml)
        {
            var fs = new StringReader(xml);
            var x = new XmlSerializer(typeof(Root));
            return (Root) x.Deserialize(fs);
        }

        public static Root Create(IGraphTarget target) { return new SyntaxDrawer(target).Draw(); }

        public SVG Svg;
    }

    sealed class ArcElement : PathElement
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

        public ArcElement(Size radii, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
            : base(true)
        {
            _radii = radii;
            _end = end;
            _xAxisRotation = xAxisRotation;
            _largeArcFlag = largeArcFlag;
            _sweepFlag = sweepFlag;
        }

        public ArcElement(int radius, Size end, bool largeArcFlag, bool sweepFlag, int xAxisRotation = 0)
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

    sealed class HomeElement : PathElement
    {
        public HomeElement()
            : base(false) { }
        internal override Size Size { get { return new Size(0, 0); } }

        internal override string FormatElement { get { return "Z M" + FormatSize; } }
    }

    sealed class LineElement : PathElement
    {
        [EnableDump]
        readonly Size _end;
        public LineElement(Size end, bool isVisible = true)
            : base(isVisible) { _end = end; }

        internal override Size Size { get { return _end; } }

        internal override string FormatElement { get { return (IsVisible ? "l" : "m") + FormatSize; } }
    }

    sealed class HorizontalLineElement : PathElement
    {
        [EnableDump]
        readonly int _length;

        public HorizontalLineElement(int length, bool isVisible = true)
            : base(isVisible) { _length = length; }

        internal override Size Size { get { return new Size(_length, 0); } }
        internal override string FormatElement { get { return IsVisible ? "h" + _length : "m" + FormatSize; } }
    }

    sealed class VerticalLineElement : PathElement
    {
        [EnableDump]
        readonly int _length;

        public VerticalLineElement(int length, bool isVisible = true)
            : base(isVisible) { _length = length; }

        internal override Size Size { get { return new Size(0, _length); } }
        internal override string FormatElement { get { return IsVisible ? "v" + _length : "m" + FormatSize; } }
    }

    abstract class PathElement : ReniObject
    {
        protected readonly bool IsVisible;

        protected PathElement(bool isVisible) { IsVisible = isVisible; }

        internal abstract Size Size { get; }
        internal abstract string FormatElement { get; }
        internal string FormatSize { get { return FormatPair(Size); } }
        protected static string FormatPair(Size size) { return size.Width + "," + size.Height; }

        internal static string Format(params PathElement[] path)
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
    }
}