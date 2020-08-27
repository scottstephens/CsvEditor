using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvEditor
{
    static class Program
    {
        private static Shared Shared;
        private static string[] Args;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if NETCOREAPP
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Args = args;
            Shared = new Shared();

            Application.Idle += Application_Idle;

            Application.Run();
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;
            var f = Shared.LaunchForm(() => new OpenFileForm(Shared, Args));
            f.Show();
        }
    }
}
