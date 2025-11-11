using System;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        public Form activeForm = null;
        public void OpenChildForm(Form childForm) {
            if (activeForm != null)
            {
                activeForm.Close();
            }
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            pnlMain.Controls.Add(childForm);
            pnlMain.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        private void ptbDepartment_Click(object sender, EventArgs e) {
            OpenChildForm(new DepartmentForm());
        }

        private void ptbEmployee_Click(object sender, EventArgs e) {
            OpenChildForm(new EmployeeForm());
        }

        private void ptbSalary_Click(object sender, EventArgs e) {
            OpenChildForm(new SalaryForm());
        }

        private void ptbLogout_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Do you want Logout ?", "Infor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Dispose();
            }
        }

        private void ptbAccount_Click(object sender, EventArgs e) {
            OpenChildForm(new AccountForm());
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.WindowState = FormWindowState.Maximized;

            // Layout that resizes correctly
            panel1.Dock = DockStyle.Top;
            panel2.Dock = DockStyle.Bottom;
            pnlMain.Dock = DockStyle.Fill;



            // Right menu stays at the right edge
            ptbEmployee.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ptbDepartment.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ptbSalary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ptbAccount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ptbLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            OpenChildForm(new IntroForm());
        }

        private void label1_Click(object sender, EventArgs e) {

        }
    }
}
