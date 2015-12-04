using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
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

        void OnClicked(FunctionId functionId)
        {
            NotImplementedMethod(functionId);
        }

        Control ResultCacheView(KeyValuePair<ContextBase, ResultCache> item)
        {
            var control = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial
            };

            control.Controls.Add(ContextView(item.Key));
            control.Controls.Add(CreateResultView(item.Value.Data));

            foreach(RowStyle rowStyle in control.RowStyles)
                rowStyle.SizeType = SizeType.AutoSize;

            return control;
        }

        Control CreateResultView(Result result)
        {
            var clients = new List<Control>();
            if(result.HasType)
                clients.Add(CreateTypeView(result.Type));
            else if(result.HasSize)
            {
                var control = result.Size.IsZero
                    ? "Hollow".CreateView()
                    : result.Size.ToString().CreateView().CreateGroup("Size");
                clients.Add(control);
            }
            else if(result.HasHllw)
                clients.Add((result.HasHllw ? "Hollow" : "Unknown size").CreateView());

            if(result.HasExts)
                clients.Add(result.Exts.CreateView());

            if(result.HasCode)
                clients.Add(result.Code.CreateView(this));

            if(clients.Any())
            {
                var resultView = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = clients.Count
                };

                resultView.Controls.AddRange(clients.ToArray());
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

        Control CreateTypeView(TypeBase type)
        {
            var text = type.NodeDump.CreateView();
            text.Click += (s, a) => OnClicked(type);
            return text.CreateGroup("Type");
        }

        void OnClicked(TypeBase target) => NotImplementedMethod(target);

        Control ContextView(ContextBase context)
        {
            var result = context.NodeDump.CreateView();
            result.Click += (s, a) => OnClicked(context);
            return result;
        }

        void OnClicked(ContextBase target) => NotImplementedMethod(target);

        internal Control CreateFunctionCallView(Call visitedObject)
        {
            var functionId = visitedObject.FunctionId;
            var name = functionId.ToString();
            var result = name.CreateView();
            var menuItem = new MenuItem
            {
                Name = name
            };
            menuItem.Click += (a, b) => OnClicked(functionId);
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(menuItem);
            result.ContextMenu = contextMenu;
            return result;
        }
    }
}