using System;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Global safety net so exceptions don’t silently terminate before a window shows
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show("Unexpected error: " + e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                    MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Do not touch the database before showing UI; avoid startup hangs if SQL Server is unavailable
            try {
                Application.Run(new LoginForm());
            } catch (Exception ex) {
                MessageBox.Show("Failed to start UI: " + ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
