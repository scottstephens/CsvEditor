using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvEditor
{
    public class Shared
    {
        public readonly string Title = "CsvEditor";

        public HashSet<Form> OpenForms = new HashSet<Form>();

        public T LaunchForm<T>(Func<T> form_builder) where T : Form
        {
            var f = form_builder();
            f.FormClosed += FormClosed;
            this.OpenForms.Add(f);
            return f;
        }

        private void FormClosed(object sender, FormClosedEventArgs e)
        {
            var f = sender as Form;
            this.OpenForms.Remove(f);
            if (this.OpenForms.Count == 0)
                Application.Exit();
        }
    }
}
