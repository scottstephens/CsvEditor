using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvEditor
{
    public partial class OpenFileForm : Form
    {
        private Shared Shared;

        public OpenFileForm(Shared shared, string[] args)
        {
            InitializeComponent();
            this.Shared = shared;
            this.Text = this.Shared.Title;
            if (args.Length == 1)
                tbFilePath.Text = args[0];
            else if (args.Length != 0)
                lblError.Text = "Error: called with an incorrect number of arguments. Only 0 and 1 are supported.";
        }

        private void OpenFileForm_Load(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var result = this.OpenFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                tbFilePath.Text = this.OpenFileDialog.FileName;
            }
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            if (!ValidateFilePath())
                return;

            string header_line = null;
            List<string> body_lines = new List<string>();
            using (var reader = new StreamReader(this.tbFilePath.Text))
            {
                int line_count = 0;
                while (!reader.EndOfStream && line_count < 100)
                {
                    line_count += 1;
                    var line = reader.ReadLine();
                    if (line_count == 1)
                        header_line = line;
                    else
                        body_lines.Add(line);
                }
            }

            Dictionary<char, int> separator_candidates = new Dictionary<char, int>();
            foreach (var c in header_line)
            {
                if (char.IsPunctuation(c) || char.IsSeparator(c) || char.IsWhiteSpace(c) || char.IsSymbol(c))
                {
                    if (separator_candidates.TryGetValue(c, out int current_value))
                        separator_candidates[c] = current_value + 1;
                    else
                        separator_candidates.Add(c, 1);
                }
            }

            int max = 0;
            char max_candidate = ',';
            foreach (var candidate in separator_candidates)
            {
                if (candidate.Value > max)
                {
                    max = candidate.Value;
                    max_candidate = candidate.Key;
                }
            }

            tbDelimiter.Text = max_candidate.ToString();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!ValidateFilePath())
                return;
            var settings = new ParseSettings()
            {
                Delimiter = this.tbDelimiter.Text,
            };

            var f = this.Shared.LaunchForm(() => new EditorForm(this.Shared, this.tbFilePath.Text, settings));
            f.Show();
            this.Close();
        }

        private bool ValidateFilePath()
        {
            if (!File.Exists(tbFilePath.Text))
            {
                lblError.Text = "Error: file not found.";
                return false;
            }
            else
            {
                lblError.Text = "";
                return true;
            }
        }
    }
}
