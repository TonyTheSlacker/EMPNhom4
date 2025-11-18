using System; // Provides basic classes and base types like EventArgs
using System.IO; // File and directory I/O
using System.Linq; // LINQ extensions for querying collections
using System.Security.Cryptography; // Cryptographic services (MD5, AES)
using System.Text; // Text encoding utilities
using System.Windows.Forms; // Windows Forms UI classes
using System.Threading.Tasks; // Task-based async pattern
using System.Data.SqlClient; // SQL Server client provider

namespace EmployeeManagementSystem { // Namespace for the application
    public partial class LoginForm : Form { // Partial class for the login form, inherits from Form
        // Remove long-lived DataContext to avoid UI-thread blocking and threading issues
        // EmployeeDataContext db = new EmployeeDataContext(); // Old persistent context (commented out)
        public LoginForm() { // Constructor for the login form
            InitializeComponent(); // Initialize WinForms controls
            txtusername.Text = ""; // Clear username textbox on load
            txtpassword.Text = ""; // Clear password textbox on load
            LoadRemembered(); // Load remembered credentials if present
        }

        private void chkshow_CheckedChanged(object sender, EventArgs e) { // Toggle show/hide password handler
            if (chkshow.Checked)
            {
                txtpassword.UseSystemPasswordChar = false; // Show password characters
            } else
            {
                txtpassword.UseSystemPasswordChar = true; // Mask password characters
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e) { // Close app when pictureBox2 is clicked
            Application.Exit(); // Exit the application
        }

        private void label5_Click(object sender, EventArgs e) { // Clear input fields when label5 is clicked
            txtpassword.ResetText(); // Reset password textbox
            txtusername.ResetText(); // Reset username textbox
        }

        private async void Login_Click(object sender, EventArgs e) { // Async click handler for the Login button

            string user = txtusername.Text.Trim(); // Read and trim the username input
            string password = txtpassword.Text.Trim(); // Read and trim the password input
            if (user == "" || password == "") // Validate required fields
            {
                MessageBox.Show("Miss data"); // Notify user about missing data
                return; // Stop processing
            }

            ToggleUi(false); // Disable UI while logging in
            try
            {
                string hashed = Hashpassword(password); // Hash the entered password to match DB format
                string baseConn = Properties.Settings.Default.EmployeeManagementSystemConnectionString ?? string.Empty; // Read base connection string from settings
                string conn = EnsureShortTimeout(baseConn,5); // seconds // Ensure connection timeout is short

                var account = await Task.Run(() => // Run DB query off UI thread
                {
                    using (var db = new EmployeeDataContext(conn)) // Create a new LINQ-to-SQL data context with short-lived scope
                    {
                        db.CommandTimeout =5; // seconds for commands // Set command timeout short
                        //so sansh
                        return db.Accounts.SingleOrDefault(p => p.username == user && p.password == hashed); // Query Accounts for a single match by username and hashed password
                    }
                });

                if (account == null) // If no account matched, login fails
                {
                    lblCheck.Visible = true; // Show feedback label
                    lblCheck.Text = "Account not correct !"; // Inform user of incorrect credentials
                }
                else // Login success path
                {
                    // Remember credentials if asked
                    if (chkRemember != null && chkRemember.Checked) // If Remember Me is checked
                    {
                        RememberMeStore.Save(user, password); // Persist username and encrypted password locally
                    }
                    else
                    {
                        RememberMeStore.Clear(); // Remove remembered credentials if not requested
                    }

                    txtpassword.ResetText(); // Clear password for security
                    txtusername.ResetText(); // Clear username field
                    Form1 frm = new Form1(); // Create main form instance
                    frm.ShowDialog(); // Show main form modally

                    // when user comes back from main form (logout/close), prefill remembered
                    LoadRemembered(); // Reload remembered credentials after returning
                }
            }
            catch (SqlException ex) when (ex.Number == -2) // SQL timeout // Catch SQL timeout exceptions specifically
            {
                MessageBox.Show("Cannot reach the database (timeout). Please check SQL Server instance and connection string.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Warn about connection timeout
            }
            catch (Exception ex) // General exception handler
            {
                MessageBox.Show("Login failed: " + ex.Message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show generic error
            }
            finally
            {
                ToggleUi(true); // Re-enable UI regardless of outcome
            }
        }

        private void ToggleUi(bool enabled)
        {
            this.Cursor = enabled ? Cursors.Default : Cursors.WaitCursor; // Show wait cursor when disabled
            if (Login != null) Login.Enabled = enabled; // Enable/disable Login button
            if (txtusername != null) txtusername.Enabled = enabled; // Enable/disable username input
            if (txtpassword != null) txtpassword.Enabled = enabled; // Enable/disable password input
        }

        private static string EnsureShortTimeout(string conn, int seconds) // Ensure the connection string has a short timeout
        {
            if (string.IsNullOrWhiteSpace(conn)) return conn; // No change if empty
            // Replace existing timeout if present; otherwise append
            var lower = conn.ToLowerInvariant(); // Lowercase for case-insensitive search
            if (lower.Contains("connect timeout=") || lower.Contains("connection timeout=")) // If a timeout exists
            {
                // simple replace numbers
                conn = System.Text.RegularExpressions.Regex.Replace(conn, "(?i)(connect(?:ion)? timeout=)\\s*\\d+", "$1" + seconds); // Replace existing timeout value
                return conn; // Return updated string
            }
            if (!conn.TrimEnd().EndsWith(";")) conn += ";"; // Ensure trailing semicolon
            return conn + "Connect Timeout=" + seconds + ";"; // Append a short timeout
        }

        private void txtusername_KeyPress(object sender, KeyPressEventArgs e) { // Hide error label when typing username
            lblCheck.Visible = false; // Hide the feedback label
        }

        private void btnForget_Click(object sender, EventArgs e) { // Open the Forget Password form
            ForgetPasswordForm f = new ForgetPasswordForm(); // Create forget password form instance
            f.ShowDialog(); // Show it modally
        }
        string Hashpassword(string pass) { // Compute password hash for comparison with DB
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()) // Create MD5 provider
            {
                UTF8Encoding utf8 = new UTF8Encoding(); // Create UTF8 encoder
                byte[] data = md5.ComputeHash(utf8.GetBytes(pass)); // Hash the UTF8 bytes of the password
                return Convert.ToBase64String(data); // Return Base64 string of MD5 hash
            }
        }

        private void LoadRemembered() { // Load remembered credentials from local store
            string user, pass; // Output vars for username and password
            if (RememberMeStore.TryLoad(out user, out pass)) // Attempt to load stored credentials
            {
                txtusername.Text = user; // Prefill username
                txtpassword.Text = pass; // Prefill password
                if (chkRemember != null)
                    chkRemember.Checked = true; // Check Remember Me if credentials were loaded
                // keep masked unless user selects show
                txtpassword.UseSystemPasswordChar = !(chkshow != null && chkshow.Checked); // Keep password masked unless show is checked
            }
        }

        // Open About Us form when the info button is clicked
        private void btnInfo_Click(object sender, EventArgs e) { // Info button click handler
            using (var about = new AboutUsForm()) // Create AboutUs form with using for disposal
            {
                about.ShowDialog(this); // Show it modally with this form as owner
            }
        }
    } // End of LoginForm class

    internal static class RememberMeStore { // Helper class to store/retrieve remember-me credentials locally
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QLNHANVIENFULL"); // App data directory path
        private static readonly string FilePath = Path.Combine(AppDir, "remember.dat"); // Path to credentials file
        // Derive a static key/IV from app-specific strings (sufficient for local remember-me)
        private static readonly byte[] Key; // AES key
        private static readonly byte[] IV; // AES IV

        static RememberMeStore() { // Static constructor to initialize crypto material
            using (var sha = SHA256.Create()) // Create SHA256 to derive key
            {
                Key = sha.ComputeHash(Encoding.UTF8.GetBytes("QLNHANVIENFULL.Remember.Key.v1")); // Derive a 32-byte key from a fixed string
            }
            using (var md5 = MD5.Create()) // Create MD5 to derive IV
            {
                IV = md5.ComputeHash(Encoding.UTF8.GetBytes("QLNHANVIENFULL.Remember.IV.v1")); // Derive 16+ bytes from a fixed string
                Array.Resize(ref IV, 16); // Ensure IV is 16 bytes for AES CBC
            }
        }

        public static void Save(string user, string password) { // Save username and encrypted password to disk
            Directory.CreateDirectory(AppDir); // Ensure app directory exists
            string enc = Encrypt(password); // Encrypt the password
            string payload = user + "|" + enc; // Compose payload "user|cipher"
            File.WriteAllText(FilePath, payload, Encoding.UTF8); // Persist payload to file
        }

        public static bool TryLoad(out string user, out string password) { // Try to load stored credentials
            user = null; // Initialize out param
            password = null; // Initialize out param
            if (!File.Exists(FilePath)) // If no file, nothing to load
                return false; // Indicate failure
            try
            {
                string text = File.ReadAllText(FilePath, Encoding.UTF8); // Read payload from file
                string[] parts = text.Split(new[] { '|' }, 2); // Split into username and cipher text
                if (parts.Length != 2) // Validate payload format
                    return false; // Invalid format
                user = parts[0]; // Extract username
                password = Decrypt(parts[1]); // Decrypt password
                return true; // Success
            } catch { return false; } // On any error, return false
        }

        public static void Clear() { // Delete stored credentials file
            if (File.Exists(FilePath)) // If file exists
            {
                try
                {
                    File.Delete(FilePath); // Delete file
                } catch { } // Ignore IO errors
            }
        }

        private static string Encrypt(string plain) { // Encrypt a plaintext string with AES-CBC
            using (var aes = Aes.Create()) // Create AES algorithm instance
            {
                aes.Key = Key; // Assign derived key
                aes.IV = IV; // Assign derived IV
                aes.Mode = CipherMode.CBC; // Use CBC mode
                aes.Padding = PaddingMode.PKCS7; // Use PKCS7 padding
                using (var ms = new MemoryStream()) // Buffer for ciphertext
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) // Crypto stream for writing
                using (var sw = new StreamWriter(cs, Encoding.UTF8)) // Stream writer to write plaintext
                {
                    sw.Write(plain); // Write plaintext into the crypto stream
                    sw.Flush(); // Flush writer buffers
                    cs.FlushFinalBlock(); // Finalize the encryption block
                    return Convert.ToBase64String(ms.ToArray()); // Return Base64 cipher text
                }
            }
        }

        private static string Decrypt(string cipherBase64) { // Decrypt a Base64-encoded AES-CBC ciphertext
            byte[] data = Convert.FromBase64String(cipherBase64); // Decode Base64 to bytes
            using (var aes = Aes.Create()) // Create AES algorithm instance
            {
                aes.Key = Key; // Assign derived key
                aes.IV = IV; // Assign derived IV
                aes.Mode = CipherMode.CBC; // Use CBC mode
                aes.Padding = PaddingMode.PKCS7; // Use PKCS7 padding
                using (var ms = new MemoryStream(data)) // Stream over cipher bytes
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read)) // Crypto stream for reading
                using (var sr = new StreamReader(cs, Encoding.UTF8)) // Reader to read decrypted text
                {
                    return sr.ReadToEnd(); // Read and return plaintext
                }
            }
        }
    } // End of RememberMeStore class
} // End of EmployeeManagementSystem namespace
