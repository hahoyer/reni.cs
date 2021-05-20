using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using Reni.Code;

namespace ReniUI.CompilationView
{
    sealed class Step : DumpableObject
    {
        internal readonly int Index;
        internal readonly IFormalCodeItem CodeBase;
        readonly DataStack.DataMemento[] Before;
        internal Exception Exception;
        DataStack.DataMemento[] After { get; set; }
        readonly FrameItem[] Frames;

        internal Step
            (
            IFormalCodeItem codeBase,
            DataStack dataStack,
            int index,
            FrameItem[] frames)
        {
            CodeBase = codeBase;
            Before = dataStack.GetLocalItemMementos().ToArray();
            Index = index;
            Frames = frames;
        }

        internal DataStack AfterStack { set { After = value.GetLocalItemMementos().ToArray(); } }

        internal DataGridViewRow CreateRowForStep()
        {
            var result = new DataGridViewRow();
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = Index
                    });
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = Frames.Length
                    });
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = CodeBase.NodeDump()
                    });
            if(Exception != null)
                result.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.Red
                };
            return result;
        }

        internal static IEnumerable<DataGridViewColumn> DataGridViewColumns()
        {
            yield return new DataGridViewTextBoxColumn
            {
                Name = "#",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            yield return new DataGridViewTextBoxColumn
            {
                Name = "Depth",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            yield return new DataGridViewTextBoxColumn
            {
                Name = "Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
        }

        internal void OnSelect(int columnIndex, SourceView master)
        {
            if(columnIndex == 0)
                master.SignalClickedStep(this);

            if(columnIndex == 2)
            {
                //master.SignalClickedCode(CodeBase);
            }
        }

        internal Control CreateView(SourceView master)
            => false.CreateLineupView
                (
                    Exception?.ToString().CreateView(),
                    CreateDataStackView(),
                    CreateStackView(master)
                );

        Control CreateDataStackView()
        {
            var data = Before
                .Merge(After, item => item.Offset)
                .OrderByDescending(item => item.Item1)
                .ToArray();

            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 5,
                RowCount = data.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for(var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                var isChange = item.Item2?.ValueDump != item.Item3?.ValueDump;

                if(item.Item2 != null)
                {
                    result.Controls.Add(item.Item2.Size.CreateView(isBold: isChange), 0, i);
                    result.Controls.Add(item.Item2.ValueDump.CreateView(isBold: isChange), 1, i);
                }

                result.Controls.Add((-item.Item1).CreateView(isBold: isChange), 2, i);

                if(item.Item3 != null)
                {
                    result.Controls.Add(item.Item3.Size.CreateView(isBold: isChange), 3, i);
                    result.Controls.Add(item.Item3.ValueDump.CreateView(isBold: isChange), 4, i);
                }
            }

            return result;
        }

        Control CreateStackView(SourceView master)
        {
            var result = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = Frames.Length,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for(var rowIndex = 0; rowIndex < Frames.Length; rowIndex++)
            {
                var item = Frames[rowIndex];
                var columnIndex = 0;
                result.Controls.Add
                    ((Frames.Length - rowIndex).CreateView(), columnIndex++, rowIndex);
                result.Controls.Add(item.FunctionId.CreateView(master), columnIndex++, rowIndex);
                result.Controls.Add(item.CallStep.CreateLink(), columnIndex++, rowIndex);
                result.Controls.Add(item.Text.CreateView(), columnIndex++, rowIndex);
            }

            return result;
        }
    }
}