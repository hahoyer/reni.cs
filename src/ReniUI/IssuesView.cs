using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Scanner;
using Reni.Validation;

namespace ReniUI
{
    sealed class IssuesView : ChildView
    {
        public IssuesView(IDataProvider provider)
            : base(provider.Master, "Issues")
        {
            Frame.Text = "Issues";

            var result = new DataGridView
            {
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.RowHeaderSelect,
                MultiSelect = true
            };
            var data = provider.Data.ToArray();

            result.Columns.AddRange(DataGridViewColumns().ToArray());
            result.Rows.AddRange(data.Select(CreateRow).ToArray());
            result.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            result.CellClick += (a, b) => OnSelect(data[b.RowIndex], provider);
            Client = result;
        }

        static void OnSelect(Issue issue, IDataProvider provider) 
            => provider.SignalClicked(issue.Position);

        static DataGridViewRow CreateRow(Issue item, int index)
        {
            var result = new DataGridViewRow();
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = index
                    });
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = item.Position.FilePosition()
                    });
            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = item.Position.Id
                    });

            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = item.Tag
                    });

            result.Cells.Add
                (
                    new DataGridViewTextBoxCell
                    {
                        Value = item.Message
                    });

            return result;
        }

        static IEnumerable<DataGridViewColumn> DataGridViewColumns()
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
                Name = "Position",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };

            yield return new DataGridViewTextBoxColumn
            {
                Name = "Char",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            yield return new DataGridViewTextBoxColumn
            {
                Name = "Tag",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            yield return new DataGridViewTextBoxColumn
            {
                Name = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
        }

        internal interface IDataProvider
        {
            IEnumerable<Issue> Data { get; }
            IApplication Master { get; }
            void SignalClicked(SourcePart position);
        }
    }
}