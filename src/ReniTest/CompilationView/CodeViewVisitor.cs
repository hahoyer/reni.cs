using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni;
using Reni.Code;

namespace ReniTest.CompilationView
{
    sealed class CodeViewVisitor : Visitor<Control, Control>
    {
        readonly ResultCachesView Parent;

        internal CodeViewVisitor(ResultCachesView parent) { Parent = parent; }

        internal override Control ContextRef(ReferenceCode visitedObject)
            => visitedObject.Context.NodeDump().CreateView();

        internal override Control BitArray(BitArray visitedObject)
            => visitedObject.Data.ToString().CreateView();

        internal override Control Call(Call visitedObject) 
            => Parent.CreateFunctionCallView(visitedObject);

        internal override Control TopRef(TopRef visitedObject)
            => ("Offset=" + visitedObject.Offset.ToInt()).CreateView();

        protected override Control Fiber(Fiber visitedObject, Control newHead, Control[] newItems)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = visitedObject.FiberItems.Length + 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            var fiberHead = visitedObject.FiberHead;
            result.Controls.Add(fiberHead.Size.ToInt().CreateView(), 1, 0);
            result.Controls.Add(fiberHead.GetType().PrettyName().CreateView(), 2, 0);
            result.Controls.Add(newHead ?? Default(fiberHead), 3, 0);

            for(var i = 0; i < visitedObject.FiberItems.Length; i++)
            {
                var item = visitedObject.FiberItems[i];
                result.Controls.Add(item.InputSize.ToInt().CreateView(), 0, i + 1);
                result.Controls.Add(item.OutputSize.ToInt().CreateView(), 1, i + 1);
                result.Controls.Add(item.GetType().PrettyName().CreateView(), 2, i + 1);
                result.Controls.Add(newItems[i] ?? item.CreateView(), 3, i + 1);
            }

            return result;
        }

        protected override Control List(List visitedObject, IEnumerable<Control> newList)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 3,
                RowCount = visitedObject.Data.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            var enumerable = newList.ToArray();
            for(var i = 0; i < visitedObject.Data.Length; i++)
            {
                var item = visitedObject.Data[i];
                result.Controls.Add(item.Size.ToInt().CreateView(), 0, i);
                result.Controls.Add(item.GetType().PrettyName().CreateView(), 1, i);
                result.Controls.Add(enumerable[i] ?? Default(item), 2, i);
            }

            return result;
        }

        internal override Control Default(CodeBase codeBase)
            => codeBase.CreateView();
    }
}