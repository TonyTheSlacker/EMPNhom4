using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    public partial class ForgetPasswordForm : Form {

        Random rd = new Random();
        int otp;

        EmployeeDataContext db = new EmployeeDataContext();
        public ForgetPasswordForm() {

            InitializeComponent();
        }

        void hide() {
            lblNewPassword.Visible = false;
            lblOTP.Visible = false;
            txtNewpassword.Visible = false;
            txtOTP.Visible = false;
            chkShowPass.Visible = false;
        }
        void show() {
            lblNewPassword.Visible = true;
            lblOTP.Visible = true;
            txtNewpassword.Visible = true;
            txtOTP.Visible = true;
            chkShowPass.Visible = true;
        }

        private void ForgetPasswordForm_Load(object sender, EventArgs e) {

        }

        private void btnConfirm_Click(object sender, EventArgs e) {
            if (otp.ToString() == txtOTP.Text.Trim())
            {
                if (txtNewpassword.Text.Length >= 6)
                {
                    Account acc = db.Accounts.SingleOrDefault(m => m.username == txtUser.Text.Trim());
                    acc.password = Hashpassword(txtNewpassword.Text);
                    db.SubmitChanges();
                    MessageBox.Show("Password Updated! Please log in again!");
                    this.Dispose();
                } else
                {
                    MessageBox.Show("Password's length must be greater than or equals to 6", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


            } else
            {
                MessageBox.Show("Incorrect OTP! Please try again!");
                return;
            }

        }

        private void btnRequest_Click(object sender, EventArgs e) {


            if (string.IsNullOrEmpty(txtUser.Text))
            {
                MessageBox.Show("Missing Username/Password!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            } else
            {
                Account acc = db.Accounts.SingleOrDefault(a => a.username == txtUser.Text.Trim());
                if (acc == null)
                {
                    MessageBox.Show("Account does not exist!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    otp = rd.Next(10000, 99999);

                    var from = new MailAddress("hoanhoan010@gmail.com");
                    var to = new MailAddress(acc.Email);

                    const string frompass = "wvki gjuv cfkd cjbs"; // app password trong myacccunt google, copy paste chứ đừng ghi dính liền
                    const string subject = "OTP CODE";
                    string body = "Your OTP to reset password is: " + otp.ToString();

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(from.Address, frompass),
                        Timeout = 200000
                    };
                    using (var message = new MailMessage(from, to)
                    {
                        Subject = subject,
                        Body = body,

                    })
                    {
                        smtp.Send(message);
                    }
                    MessageBox.Show("OTP Sent Successfully! Please check your email!q");
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    hide();
                    return;
                }
                show();
            }
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e) {
            if (chkShowPass.Checked)
            {
                txtNewpassword.UseSystemPasswordChar = false;
            } else
            {
                txtNewpassword.UseSystemPasswordChar = true;
            }


        }

        private void pictureBox3_Click(object sender, EventArgs e) {
            this.Dispose();
        }
        string Hashpassword(string pass) {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] data = md5.ComputeHash(utf8.GetBytes(pass));
                return Convert.ToBase64String(data);
            }
        }
    }
}
