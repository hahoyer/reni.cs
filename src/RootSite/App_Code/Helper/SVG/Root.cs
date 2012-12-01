#region Copyright (C) 2012

//     Project RootSite
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HWClassLibrary.Debug;
using Reni.Graphics;

namespace Helper.SVG
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

        public static Root Create(IGraphTarget syntax)
        {
            var contents = new Content[]
            {
                new Rect
                {
                    X = 1,
                    Y = 1,
                    Width = 1198,
                    Height = 398,
                    Fill = "none",
                    Stroke = "blue",
                    StrokeWidth = 10
                },
                new Circle
                {
                    Cx = 600,
                    Cy = 200,
                    R = 100,
                    Fill = "red",
                    Stroke = "blue",
                    StrokeWidth = 10
                }
            };
            var svg = new SVG
            {
                Width = "12cm", Height = "4cm", ViewBox = "0 0 1200 400",
            };

            svg.Items.AddRange(contents);
            return new Root {Svg = svg};
        }

        public SVG Svg;
    }
}