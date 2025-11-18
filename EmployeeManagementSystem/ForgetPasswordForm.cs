using System; // Base types and events
using System.Linq; // LINQ for querying tables
using System.Net; // Network credential classes
using System.Net.Mail; // SMTP email sending classes
using System.Security.Cryptography; // Cryptography (MD5 hashing)
using System.Text; // Text encoding utilities
using System.Windows.Forms; // WinForms UI framework

namespace EmployeeManagementSystem { // Application namespace
    public partial class ForgetPasswordForm : Form { // Form for password reset workflow

        Random rd = new Random(); // Random number generator for OTP
        int otp; // Stores the currently generated OTP code

        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context (uses app connection string)
        public ForgetPasswordForm() { // Constructor

            InitializeComponent(); // Initialize designer-created controls
        }

        void hide() { // Hide OTP and new password UI elements
            lblNewPassword.Visible = false; // Hide new password label
            lblOTP.Visible = false; // Hide OTP label
            txtNewpassword.Visible = false; // Hide new password textbox
            txtOTP.Visible = false; // Hide OTP textbox
            chkShowPass.Visible = false; // Hide show password checkbox
        }
        void show() { // Show OTP and new password UI elements
            lblNewPassword.Visible = true; // Show new password label
            lblOTP.Visible = true; // Show OTP label
            txtNewpassword.Visible = true; // Show new password textbox
            txtOTP.Visible = true; // Show OTP textbox
            chkShowPass.Visible = true; // Show show password checkbox
        }

        private void ForgetPasswordForm_Load(object sender, EventArgs e) { // Form load event placeholder

        }

        private void btnConfirm_Click(object sender, EventArgs e) { // Confirm button handler (validate OTP and update password)
            if (otp.ToString() == txtOTP.Text.Trim()) // Check if entered OTP matches generated OTP
            {
                if (txtNewpassword.Text.Length >= 6) // Ensure new password length is at least 6
                {
                    Account acc = db.Accounts.SingleOrDefault(m => m.username == txtUser.Text.Trim()); // Fetch account by username
                    acc.password = Hashpassword(txtNewpassword.Text); // Hash and set new password
                    db.SubmitChanges(); // Persist password change to database
                    MessageBox.Show("Password Updated! Please log in again!"); // Notify success
                    this.Dispose(); // Close the form
                } else // Password too short
                {
                    MessageBox.Show("Password's length must be greater than or equals to 6", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Show validation message
                    return; // Exit 
                }


            } else // OTP not match
            {
                MessageBox.Show("Incorrect OTP! Please try again!"); // mismatch
                return; // Exit 
            }

        }

        private void btnRequest_Click(object sender, EventArgs e) { // Request OTP button click handler


            if (string.IsNullOrEmpty(txtUser.Text)) // Validate username presence
            {
                MessageBox.Show("Missing Username/Password!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Alert missing data
                return; // Exit handler
            } else // Username provided
            {
                Account acc = db.Accounts.SingleOrDefault(a => a.username == txtUser.Text.Trim()); // Look up account
                if (acc == null) // Account not found
                {
                    MessageBox.Show("Account does not exist!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Notify user
                    return; // Exit handler
                }
                try // Attempt to send OTP email
                {
                    otp = rd.Next(10000, 99999); // Generate a 5-digit OTP

                    var from = new MailAddress("hoanhoan010@gmail.com"); // Sender email address
                    var to = new MailAddress(acc.Email); // Recipient email (from account record)

                    const string frompass = "wvki gjuv cfkd cjbs"; // Gmail app password (should be stored securely; here hard-coded)
                    const string subject = "OTP CODE"; // Email subject line
                    string body = "Your OTP to reset password is: " + otp.ToString(); // Email body with OTP

                    var smtp = new SmtpClient // Configure SMTP client
                    {
                        Host = "smtp.gmail.com", // Gmail SMTP host
                        Port = 587, // TLS port
                        EnableSsl = true, // Enable SSL/TLS
                        DeliveryMethod = SmtpDeliveryMethod.Network, // Network delivery
                        UseDefaultCredentials = false, // Custom credentials
                        Credentials = new NetworkCredential(from.Address, frompass), // Auth credentials
                        Timeout = 200000 // Timeout in milliseconds
                    };
                    using (var message = new MailMessage(from, to) // Create mail message object
                    {
                        Subject = subject, // Set subject
                        Body = body, // Set body

                    })
                    {
                        smtp.Send(message); // Send the email
                    }
                    MessageBox.Show("OTP Sent Successfully! Please check your email!"); // Notify success
                } catch (Exception ex) // Handle errors during send
                {
                    MessageBox.Show(ex.ToString()); // Show exception details
                    hide(); // Hide password reset controls
                    return; // Exit handler
                }
                show(); // Reveal password reset controls after sending email
            }
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e) { // Show/hide password checkbox handler
            if (chkShowPass.Checked) // If show password selected
            {
                txtNewpassword.UseSystemPasswordChar = false; // Unmask password text
            } else // Hide password
            {
                txtNewpassword.UseSystemPasswordChar = true; // Mask password text
            }


        }
        private void pictureBox3_Click(object sender, EventArgs e) { // Close icon click handler
            this.Dispose(); // Close and dispose form
        }
        string Hashpassword(string pass) { // Hash password helper (MD5 + Base64)
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()) // Create MD5 provider
            {
                UTF8Encoding utf8 = new UTF8Encoding(); // UTF8 encoder
                byte[] data = md5.ComputeHash(utf8.GetBytes(pass)); // Hash UTF8 bytes
                return Convert.ToBase64String(data); // Return Base64 representation
            }
        }
    }
}
