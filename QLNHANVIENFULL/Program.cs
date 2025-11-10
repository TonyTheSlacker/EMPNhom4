using System;
using System.Windows.Forms;

namespace QLNHANVIENFULL {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //test
            EmployeeDataContext db = new EmployeeDataContext();
            if (!db.DatabaseExists())
            {
                db.CreateDatabase();
            }
            // end test
            Application.Run(new LoginForm());
        }
    }
}
