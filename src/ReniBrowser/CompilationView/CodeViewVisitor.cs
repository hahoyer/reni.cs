using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni.Code;

namespace ReniBrowser.CompilationView
{
    sealed class CodeViewVisitor : Visitor<Control, Control>
    {
        readonly SourceView Master;

        internal CodeViewVisitor(SourceView master) { Master = master; }

        internal override Control ContextRef(ReferenceCode visitedObject)
            => visitedObject.Target.CreateLink(Master);

        internal override Control LocalReference(LocalReference target)
        {
            var result = new[]
            {
                target.ValueType.CreateTypeLineView(Master).CreateGroup("Type"),
                target.AlignedValueCode.CreateView(Master)
            };

            return result.CreateRowView();
        }

        internal override Control Call(Call visitedObject)
            => visitedObject.CreateView(Master);

        internal override Control TopRef(TopRef visitedObject)
            => ("Offset=" + visitedObject.Offset.ToInt()).CreateView();

        internal override Control TopFrameData(TopFrameData visitedObject)
        {
            var text = "Offset=" + visitedObject.Offset.ToInt();
            if(visitedObject.Size != visitedObject.DataSize)
                text += " DataSize=" + visitedObject.DataSize.ToInt();
            return text.CreateView();
        }

        internal override Control TopData(TopData visitedObject)
        {
            var text = "Offset=" + visitedObject.Offset.ToInt();
            if(visitedObject.Size != visitedObject.DataSize)
                text += " DataSize=" + visitedObject.DataSize.ToInt();
            return text.CreateView();
        }

        internal override Control DePointer(DePointer visitedObject)
        {
            var text = "";
            if(visitedObject.OutputSize != visitedObject.DataSize)
                text += "DataSize=" + visitedObject.DataSize.ToInt();
            return text.CreateView();
        }

        internal override Control BitCast(BitCast visitedObject)
        {
            var text = "";
            if(visitedObject.InputSize != visitedObject.InputDataSize)
                text += "InputDataSize=" + visitedObject.InputDataSize.ToInt();
            return text.CreateView();
        }

        internal override Control ReferencePlusConstant(ReferencePlusConstant visitedObject)
            => visitedObject.Right.CreateView();

        internal override Control BitArrayBinaryOp(BitArrayBinaryOp visitedObject)
            => (
                "OpToken=" + visitedObject.OpToken +
                    " LeftSize=" + visitedObject.LeftSize.ToInt() +
                    " RightSize=" + visitedObject.RightSize.ToInt()
                ).CreateView();

        internal override Control DumpPrintNumberOperation(DumpPrintNumberOperation visitedObject)
            => (
                visitedObject.RightSize.IsZero
                    ? ""
                    : " LeftSize=" + visitedObject.LeftSize.ToInt() +
                        " RightSize=" + visitedObject.RightSize.ToInt()
                ).CreateView();

        internal override Control ArraySetter(ArraySetter visitedObject)
            => ArrayAccess(visitedObject);

        internal override Control ArrayGetter(ArrayGetter visitedObject)
            => ArrayAccess(visitedObject);

        internal override Control Assign(Assign visitedObject)
            => ("TargetSize=" + visitedObject.TargetSize).CreateView();

        internal override Control DumpPrintText(DumpPrintText visitedObject)
            => visitedObject.Value.Quote().CreateView();

        internal override Control DumpPrintTextOperation(DumpPrintTextOperation visitedObject)
            => (
            "ItemSize=" + visitedObject.ItemSize +
            " Count=" + visitedObject.InputSize / visitedObject.ItemSize
            ).CreateView();

        static Control ArrayAccess(ArrayAccess visitedObject)
            => (
                "ElementSize=" + visitedObject.ElementSize +
                    " IndexSize=" + visitedObject.IndexSize
                ).CreateView();

        internal override Control Drop(Drop visitedObject) => "".CreateView();

        internal override Control BitArray(BitArray visitedObject)
            => visitedObject.Data.ToString().CreateView();

        protected override Control ThenElse
            (ThenElse visitedObject, Control newThen, Control newElse)
        {
            var thenView = newThen ?? Default(visitedObject.ThenCode);
            var elseView = newElse ?? Default(visitedObject.ElseCode);
            return true.CreateLineupView(thenView, elseView);
        }

        protected override Control Fiber(Fiber visitedObject, Control newHead, Control[] newItems)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 5,
                RowCount = visitedObject.FiberItems.Length + 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            var fiberHead = visitedObject.FiberHead;
            result.Controls.Add(fiberHead.Size.CreateView(), 1, 0);
            result.Controls.Add(fiberHead.GetType().Name.CreateView(), 2, 0);
            result.Controls.Add(newHead ?? Default(fiberHead), 3, 0);
            result.Controls.Add(Master.TraceLogItem(fiberHead).CreateLink(), 4, 0);

            for(var i = 0; i < visitedObject.FiberItems.Length; i++)
            {
                var item = visitedObject.FiberItems[i];
                result.Controls.Add(item.InputSize.CreateView(), 0, i + 1);
                result.Controls.Add(item.OutputSize.CreateView(), 1, i + 1);
                result.Controls.Add(item.GetType().Name.CreateView(), 2, i + 1);
                result.Controls.Add(newItems[i] ?? item.CreateView(), 3, i + 1);
                result.Controls.Add(Master.TraceLogItem(item).CreateLink(), 4, i + 1);
            }

            return result;
        }

        protected override Control List(List visitedObject, IEnumerable<Control> newList)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = visitedObject.Data.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            var enumerable = newList.ToArray();
            for(var i = 0; i < visitedObject.Data.Length; i++)
            {
                var item = visitedObject.Data[i];
                result.Controls.Add(item.Size.CreateView(), 0, i);
                result.Controls.Add(item.GetType().Name.CreateView(), 1, i);
                result.Controls.Add(enumerable[i] ?? Default(item), 2, i);
                result.Controls.Add(Master.TraceLogItem(item).CreateLink(), 3, i);
            }

            return result;
        }

        internal override Control Default(CodeBase codeBase)
            => codeBase.CreateView();
    }
}