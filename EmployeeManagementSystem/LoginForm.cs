using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EmployeeManagementSystem {
    public partial class LoginForm : Form {
        // Remove long-lived DataContext to avoid UI-thread blocking and threading issues
        // EmployeeDataContext db = new EmployeeDataContext();
        public LoginForm() {
            InitializeComponent();
            txtusername.Text = "";
            txtpassword.Text = "";
            LoadRemembered();
        }

        private void chkshow_CheckedChanged(object sender, EventArgs e) {
            if (chkshow.Checked)
            {
                txtpassword.UseSystemPasswordChar = false;
            } else
            {
                txtpassword.UseSystemPasswordChar = true;
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void label5_Click(object sender, EventArgs e) {
            txtpassword.ResetText();
            txtusername.ResetText();
        }

        private async void Login_Click(object sender, EventArgs e) {

            string user = txtusername.Text.Trim();
            string password = txtpassword.Text.Trim();
            if (user == "" || password == "")
            {
                MessageBox.Show("Miss data");
                return;
            }

            ToggleUi(false);
            try
            {
                string hashed = Hashpassword(password);
                string baseConn = Properties.Settings.Default.EmployeeManagementSystemConnectionString ?? string.Empty;
                string conn = EnsureShortTimeout(baseConn,5); // seconds

                var account = await Task.Run(() =>
                {
                    using (var db = new EmployeeDataContext(conn))
                    {
                        db.CommandTimeout =5; // seconds for commands
                        return db.Accounts.SingleOrDefault(p => p.username == user && p.password == hashed);
                    }
                });

                if (account == null)
                {
                    lblCheck.Visible = true;
                    lblCheck.Text = "Account not correct !";
                }
                else
                {
                    // Remember credentials if asked
                    if (chkRemember != null && chkRemember.Checked)
                    {
                        RememberMeStore.Save(user, password);
                    }
                    else
                    {
                        RememberMeStore.Clear();
                    }

                    txtpassword.ResetText();
                    txtusername.ResetText();
                    Form1 frm = new Form1();
                    frm.ShowDialog();

                    // when user comes back from main form (logout/close), prefill remembered
                    LoadRemembered();
                }
            }
            catch (SqlException ex) when (ex.Number == -2) // SQL timeout
            {
                MessageBox.Show("Cannot reach the database (timeout). Please check SQL Server instance and connection string.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private void ToggleUi(bool enabled)
        {
            this.Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
            if (Login != null) Login.Enabled = enabled;
            if (txtusername != null) txtusername.Enabled = enabled;
            if (txtpassword != null) txtpassword.Enabled = enabled;
        }

        private static string EnsureShortTimeout(string conn, int seconds)
        {
            if (string.IsNullOrWhiteSpace(conn)) return conn;
            // Replace existing timeout if present; otherwise append
            var lower = conn.ToLowerInvariant();
            if (lower.Contains("connect timeout=") || lower.Contains("connection timeout="))
            {
                // simple replace numbers
                conn = System.Text.RegularExpressions.Regex.Replace(conn, "(?i)(connect(?:ion)? timeout=)\\s*\\d+", "$1" + seconds);
                return conn;
            }
            if (!conn.TrimEnd().EndsWith(";")) conn += ";";
            return conn + "Connect Timeout=" + seconds + ";";
        }

        private void txtusername_KeyPress(object sender, KeyPressEventArgs e) {
            lblCheck.Visible = false;
        }

        private void btnForget_Click(object sender, EventArgs e) {
            ForgetPasswordForm f = new ForgetPasswordForm();
            f.ShowDialog();
        }
        string Hashpassword(string pass) {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] data = md5.ComputeHash(utf8.GetBytes(pass));
                return Convert.ToBase64String(data);
            }
        }

        private void LoadRemembered() {
            string user, pass;
            if (RememberMeStore.TryLoad(out user, out pass))
            {
                txtusername.Text = user;
                txtpassword.Text = pass;
                if (chkRemember != null)
                    chkRemember.Checked = true;
                // keep masked unless user selects show
                txtpassword.UseSystemPasswordChar = !(chkshow != null && chkshow.Checked);
            }
        }

        // Open About Us form when the info button is clicked
        private void btnInfo_Click(object sender, EventArgs e) {
            using (var about = new AboutUsForm())
            {
                about.ShowDialog(this);
            }
        }
    }

    internal static class RememberMeStore {
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QLNHANVIENFULL");
        private static readonly string FilePath = Path.Combine(AppDir, "remember.dat");
        // Derive a static key/IV from app-specific strings (sufficient for local remember-me)
        private static readonly byte[] Key;
        private static readonly byte[] IV;

        static RememberMeStore() {
            using (var sha = SHA256.Create())
            {
                Key = sha.ComputeHash(Encoding.UTF8.GetBytes("QLNHANVIENFULL.Remember.Key.v1"));
            }
            using (var md5 = MD5.Create())
            {
                IV = md5.ComputeHash(Encoding.UTF8.GetBytes("QLNHANVIENFULL.Remember.IV.v1"));
                Array.Resize(ref IV, 16);
            }
        }

        public static void Save(string user, string password) {
            Directory.CreateDirectory(AppDir);
            string enc = Encrypt(password);
            string payload = user + "|" + enc;
            File.WriteAllText(FilePath, payload, Encoding.UTF8);
        }

        public static bool TryLoad(out string user, out string password) {
            user = null;
            password = null;
            if (!File.Exists(FilePath))
                return false;
            try
            {
                string text = File.ReadAllText(FilePath, Encoding.UTF8);
                string[] parts = text.Split(new[] { '|' }, 2);
                if (parts.Length != 2)
                    return false;
                user = parts[0];
                password = Decrypt(parts[1]);
                return true;
            } catch { return false; }
        }

        public static void Clear() {
            if (File.Exists(FilePath))
            {
                try
                {
                    File.Delete(FilePath);
                } catch { }
            }
        }

        private static string Encrypt(string plain) {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs, Encoding.UTF8))
                {
                    sw.Write(plain);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string Decrypt(string cipherBase64) {
            byte[] data = Convert.FromBase64String(cipherBase64);
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var ms = new MemoryStream(data))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
