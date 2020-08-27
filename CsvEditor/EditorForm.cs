using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvEditor
{
    public partial class EditorForm : Form
    {
        private Shared Shared;
        public string FilePath;
        public ParseSettings ParseSettings;
        private List<string> Header;
        private List<List<string>> Rows;
        private int RowInEditIndex;
        private List<string> RowInEdit;
        private bool rowScopeCommit = false;
        private int cmsColumnHeader_ColumnIndex;

        public EditorForm(Shared shared, string file_path, ParseSettings parse_settings)
        {
            InitializeComponent();
            this.Init();
            this.Shared = shared;
            this.FilePath = file_path;
            this.ParseSettings = parse_settings;
            this.SetTitle(false);
            this.DataGridView.CellContextMenuStripNeeded += DataGridView_CellContextMenuStripNeeded;
        }

        private ToolStripMenuItem tsmiFreeze;
        private ToolStripMenuItem tsmiUnfreeze;
        private ToolStripMenuItem tsmiHide;
        private ToolStripMenuItem tsmiUnhideLeft;
        private ToolStripMenuItem tsmiUnhideRight;

        private void Init()
        {
            this.tsmiFreeze = new ToolStripMenuItem();
            this.tsmiFreeze.Name = nameof(this.tsmiFreeze);
            this.tsmiFreeze.Text = "Freeze";
            this.tsmiFreeze.Click += TsmiFreeze_Click;

            this.tsmiUnfreeze = new ToolStripMenuItem();
            this.tsmiUnfreeze.Name = nameof(this.tsmiUnfreeze);
            this.tsmiUnfreeze.Text = "Unfreeze";
            this.tsmiUnfreeze.Click += TsmiUnfreeze_Click; ;

            this.tsmiHide = new ToolStripMenuItem();
            this.tsmiHide.Name = nameof(this.tsmiHide);
            this.tsmiHide.Text = "Hide";
            this.tsmiHide.Click += TsmiHide_Click;

            this.tsmiUnhideLeft = new ToolStripMenuItem();
            this.tsmiUnhideLeft.Name = nameof(this.tsmiUnhideLeft);
            this.tsmiUnhideLeft.Text = "Unhide Left";
            this.tsmiUnhideLeft.Click += TsmiUnhideLeft_Click;

            this.tsmiUnhideRight = new ToolStripMenuItem();
            this.tsmiUnhideRight.Name = nameof(this.tsmiUnhideRight);
            this.tsmiUnhideRight.Text = "Unhide Right";
            this.tsmiUnhideRight.Click += TsmiUnhideRight_Click;
        }

        private void TsmiUnhideRight_Click(object sender, EventArgs e)
        {
            for (int ii = this.cmsColumnHeader_ColumnIndex + 1; ii < this.DataGridView.Columns.Count; ++ii)
            {
                if (this.DataGridView.Columns[ii].Visible)
                    break;
                else
                    this.DataGridView.Columns[ii].Visible = true;
            }
        }

        private void TsmiUnhideLeft_Click(object sender, EventArgs e)
        {
            for (int ii = this.cmsColumnHeader_ColumnIndex - 1; ii >= 0; --ii)
            {
                if (this.DataGridView.Columns[ii].Visible)
                    break;
                else
                    this.DataGridView.Columns[ii].Visible = true;
            }
        }

        private void TsmiHide_Click(object sender, EventArgs e)
        {
            this.DataGridView.Columns[this.cmsColumnHeader_ColumnIndex].Visible = false;
        }

        private void TsmiUnfreeze_Click(object sender, EventArgs e)
        {
            for (int ii = this.cmsColumnHeader_ColumnIndex; ii >= 0; --ii)
                this.DataGridView.Columns[ii].Frozen = false;
        }

        private void TsmiFreeze_Click(object sender, EventArgs e)
        {
            this.DataGridView.Columns[this.cmsColumnHeader_ColumnIndex].Frozen = true;
        }

        private void DataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                this.cmsColumnHeader.Items.Clear();
                if (this.DataGridView.Columns[e.ColumnIndex].Frozen)
                    this.cmsColumnHeader.Items.Add(this.tsmiUnfreeze);
                else
                    this.cmsColumnHeader.Items.Add(this.tsmiFreeze);
                
                this.cmsColumnHeader.Items.Add(this.tsmiHide);
                for (int ii = e.ColumnIndex - 1; ii >= 0; --ii)
                {
                    if (!this.DataGridView.Columns[ii].Visible)
                    {
                        this.cmsColumnHeader.Items.Add(this.tsmiUnhideLeft);
                        break;
                    }
                }
                for (int ii = e.ColumnIndex + 1; ii < this.DataGridView.Columns.Count; ++ii)
                {
                    if (!this.DataGridView.Columns[ii].Visible)
                    {
                        this.cmsColumnHeader.Items.Add(this.tsmiUnhideRight);
                        break;
                    }
                }

                e.ContextMenuStrip = this.cmsColumnHeader;
                this.cmsColumnHeader_ColumnIndex = e.ColumnIndex;
            }
        }

        private void SetTitle(bool has_changes)
        {
            if (!has_changes)
                this.Text = $"{this.Shared.Title} - {this.FilePath}";
            else
                this.Text = $"{this.Shared.Title} - *{this.FilePath}"; 
        }

        private void Parse(object unused)
        {
            this.Rows = new List<List<string>>();
            using (var reader = new StreamReader(this.FilePath))
            {
                int line_count = 0;
                while (!reader.EndOfStream)
                {
                    line_count += 1;
                    var line = reader.ReadLine();

                    if (line_count == 1)
                    {
                        this.Header = line.Split(new string[] { this.ParseSettings.Delimiter }, StringSplitOptions.None).ToList();
                    }
                    else
                    {
                        var row = line.Split(new string[] { this.ParseSettings.Delimiter }, StringSplitOptions.None).ToList();
                        for (int ii = this.Header.Count; ii < row.Count; ++ii)
                            this.Header.Add(null);
                        this.Rows.Add(row);
                    }
                }
            }
            
            this.BeginInvoke(new Action(this.BuildGrid));
        }

        private void BuildGrid()
        {
            this.RowInEditIndex = -1;
            this.RowInEdit = null;
            int header_index = -1;
            foreach (var header in this.Header)
            {
                header_index += 1;

                var col = new DataGridViewTextBoxColumn();
                col.HeaderText = header ?? "";
                col.Name = $"col{header_index}";
                this.DataGridView.Columns.Add(col);
            }
            this.DataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.DataGridView.RowCount = this.Rows.Count + 1; // include the new record row
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.DataGridView.VirtualMode = true;
            this.DataGridView.CellValueNeeded += DataGridView_CellValueNeeded;
            this.DataGridView.CellValuePushed += DataGridView_CellValuePushed;
            this.DataGridView.NewRowNeeded += DataGridView_NewRowNeeded;
            this.DataGridView.RowValidated += DataGridView_RowValidated;
            this.DataGridView.RowDirtyStateNeeded += DataGridView_RowDirtyStateNeeded;
            this.DataGridView.CancelRowEdit += DataGridView_CancelRowEdit;
            this.DataGridView.UserDeletingRow += DataGridView_UserDeletingRow;
            ThreadPool.QueueUserWorkItem(this.Parse, null);
        }

        private void DataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            // If new record row, no values needed
            if (e.RowIndex == this.DataGridView.RowCount - 1) return;

            List<string> temp_row;
            if (e.RowIndex == this.RowInEditIndex)
            {
                temp_row = this.RowInEdit;
            }
            else
            {
                temp_row = this.Rows[e.RowIndex];
            }

            if (e.ColumnIndex < temp_row.Count)
                e.Value = temp_row[e.ColumnIndex];
            else
                e.Value = "";
        }

        private void DataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            List<string> temp_row = null;

            if (e.RowIndex < this.Rows.Count)
            {
                if (this.RowInEdit == null)
                    this.RowInEdit = CopyRow(this.Rows[e.RowIndex]);
                this.RowInEditIndex = e.RowIndex;
                temp_row = this.RowInEdit;
            }
            else
            {
                temp_row = this.RowInEdit;
            }

            temp_row[e.ColumnIndex] = e.Value as string;
        }

        private List<string> CreateNewRow()
        {
            var new_row = new List<string>();
            foreach (var header in this.Header)
                new_row.Add("");
            return new_row;
        }

        private static List<string> CopyRow(List<string> input)
        {
            var output = new List<string>(input.Count);
            output.AddRange(input);
            return output;
        }

        private void DataGridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            this.RowInEdit = this.CreateNewRow();
            this.RowInEditIndex = this.DataGridView.Rows.Count - 1;
        }

        private void DataGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= this.Rows.Count && e.RowIndex != this.DataGridView.Rows.Count -1 )
            {
                this.Rows.Add(this.RowInEdit);
                this.RowInEdit = null;
                this.RowInEditIndex = -1;
                this.SetTitle(true);
            }
            else if (this.RowInEdit != null && e.RowIndex < this.Rows.Count)
            {
                this.Rows[e.RowIndex] = this.RowInEdit;
                this.RowInEdit = null;
                this.RowInEditIndex = -1;
                this.SetTitle(true);
            }
            else if (this.DataGridView.ContainsFocus)
            {
                this.RowInEdit = null;
                this.RowInEditIndex = -1;
            }
        }

        private void DataGridView_RowDirtyStateNeeded(object sender, QuestionEventArgs e)
        {
            if (!rowScopeCommit)
            {
                e.Response = this.DataGridView.IsCurrentCellDirty;
            }
        }

        private void DataGridView_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            if (this.RowInEditIndex == this.DataGridView.Rows.Count - 2 && this.RowInEditIndex == this.Rows.Count)
            {
                this.RowInEdit = this.CreateNewRow();
            }
            else
            {
                this.RowInEdit = null;
                this.RowInEditIndex = -1;
            }
        }

        private void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Index < this.Rows.Count)
            {
                this.Rows.RemoveAt(e.Row.Index);
            }

            if (e.Row.Index == this.RowInEditIndex)
            {
                this.RowInEditIndex = -1;
                this.RowInEdit = null;
            }
            this.SetTitle(true);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Save(this.FilePath);
        }

        private void Save(string dest_path)
        {
            using (var writer = new StreamWriter(dest_path))
            {
                var header_line = String.Join(this.ParseSettings.Delimiter, this.Header);
                writer.WriteLine(header_line);

                foreach (var row in this.Rows)
                {
                    var line = String.Join(this.ParseSettings.Delimiter, row);
                    writer.WriteLine(line);
                }
            }
            this.SetTitle(false);
            tslStatus.Text = $"Saved {dest_path}";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = this.Shared.LaunchForm(() => new OpenFileForm(this.Shared, new string[] { }));
            f.Show();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = this.SaveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.Save(this.SaveFileDialog.FileName);
            }
        }

        private void EditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
            {
                this.Save(this.FilePath);
            }
        }

        
    }
}
