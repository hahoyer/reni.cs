using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using Reni;
using Reni.Code;

namespace ReniTest.CompilationView
{
    static class ViewExtension
    {
        internal static Control CreateGroup(this Control client, string title)
        {
            var result = new GroupBox
            {
                Text = title,
                AutoSize = true,
                Dock = DockStyle.Top,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            client.Location = result.DisplayRectangle.Location;

            result.Controls.Add(client);
            return result;
        }

        internal static Control CreateView(this CodeBase code, ResultCachesView parent)
            => CreateGroup(code.Visit(new CodeViewVisitor(parent)), "Code");

        internal static Control CreateView(this CodeArgs exts)
        {
            if(exts.Count == 0)
                return CreateView("");

            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = exts.Count,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            var items = exts
                .Data
                .Select(item => CreateView(item.NodeDump()))
                .Cast<Control>()
                .ToArray();
            result.Controls.AddRange(items);

            return result;
        }

        internal static Control CreateView(this Dumpable dumpable)
            => CreateView(dumpable.Dump());

        internal static Label CreateView(this string text)
            => new Label
            {
                Font = new Font("Lucida Console", 10),
                AutoSize = true,
                Text = text
            };

        internal static Label CreateView(this int value)
            => new Label
            {
                Font = new Font("Lucida Console", 10),
                AutoSize = true,
                Text = value.ToString(),
                TextAlign = ContentAlignment.MiddleRight
            };
    }
}