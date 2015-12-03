using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Helper;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace ReniTest.CompilationView
{
    sealed class ResultCachesView : ChildView
    {
        public ResultCachesView(CompileSyntax syntax, MainView master)
            : base(master, syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Client = CreateResultCachesView(syntax);
        }

        TableLayoutPanel CreateResultCachesView(CompileSyntax syntax)
        {
            var resultCacheViews =
                syntax
                    .ResultCache
                    .Select(ResultCacheView)
                    .ToArray();

            var client = new TableLayoutPanel
            {
                ColumnCount = resultCacheViews.Length,
                RowCount = 1
            };

            var styles = resultCacheViews
                .Select
                (
                    item => new ColumnStyle(SizeType.Percent)
                    {
                        Width = 100 / resultCacheViews.Length
                    }
                );

            foreach(var item in styles)
                client.ColumnStyles.Add(item);

            client.Controls.AddRange(resultCacheViews);
            return client;
        }

        Control ResultCacheView(KeyValuePair<ContextBase, ResultCache> item)
        {
            var control = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            control.Controls.Add(ContextView(item.Key));
            control.Controls.Add(ResultView(item.Value.Data));

            foreach(RowStyle rowStyle in control.RowStyles)
                rowStyle.SizeType = SizeType.AutoSize;

            return control;
        }

        Control ResultView(Result result)
        {
            if(result.HasType && result.HasCode && result.HasExts)
            {
                var resultView = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                resultView.Controls.Add(TypeView(result.Type));
                resultView.Controls.Add(CodeView(result.Code));
                resultView.Controls.Add(ExtsView(result.Exts));
                return resultView;
            }

            NotImplementedFunction(result);

            return new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Lucida Console", 10),
                Text = result.Dump()
            };
        }

        Control TypeView(TypeBase type)
        {
            var result = TextView(type.NodeDump);
            result.Click += (s, a) => OnTypeClicked();
            return result;
        }

        void OnTypeClicked()
        {
            NotImplementedMethod();
        }

        static Control CodeView(CodeBase code)
            => DumpableView(code);

        static Control ExtsView(CodeArgs exts)
            => DumpableView(exts);

        Control ContextView(ContextBase context)
        {
            var result = TextView(context.NodeDump);
            result.Click += (s, a) => OnContextClicked();
            return result;
        }

        void OnContextClicked()
        {
            NotImplementedMethod();
        }

        static Label DumpableView(Dumpable dumpable)
            => TextView(dumpable.Dump());

        static Label TextView(string text)
            => new Label
            {
                Font = new Font("Lucida Console", 10),
                AutoSize = true,
                Text = text
            };
    }
}