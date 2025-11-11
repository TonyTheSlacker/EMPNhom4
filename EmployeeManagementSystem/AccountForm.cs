using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    public partial class AccountForm : Form {
        EmployeeDataContext db = new EmployeeDataContext();
        public AccountForm() {
            InitializeComponent();
        }

        public void LoaddgvAccount() {
            dgvAccount.DataSource = db.Accounts.Select(a => new { a.ID, a.username, a.password, a.Email });
            lbltotal.Text = dgvAccount.RowCount.ToString();
        }
        private void AccountForm_Load(object sender, EventArgs e) {
            LoaddgvAccount();
        }

        private void ptbAdd_Click(object sender, EventArgs e) {
            UpdateAccountForm f = new UpdateAccountForm();
            f.ShowDialog();
            LoaddgvAccount();
        }

        private void dgvAccount_CellClick(object sender, DataGridViewCellEventArgs e) {
            int posrow = e.RowIndex;
            int poscol = e.ColumnIndex;
            if (posrow >= 0 && poscol >= 0)
            {
                if (dgvAccount.Columns[poscol].Name == "Delete")
                {
                    if (MessageBox.Show("Are you sure delete this Account ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {

                        Account acc = db.Accounts.SingleOrDefault(m => m.ID == int.Parse(dgvAccount["IDAcc", posrow].Value.ToString()));
                        db.Accounts.DeleteOnSubmit(acc);

                        db.SubmitChanges();
                        LoaddgvAccount();
                    }

                } else if (dgvAccount.Columns[poscol].Name == "Edit")
                {
                    Account acc = db.Accounts.SingleOrDefault(m => m.ID == int.Parse(dgvAccount["IDAcc", posrow].Value.ToString()));
                    UpdateAccountForm f = new UpdateAccountForm(acc);
                    f.btnAdd.Enabled = false;
                    f.btnUpdate.Enabled = true;
                    f.ShowDialog();
                    LoaddgvAccount();
                }

            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) {
            if (txtSearch.Text == "")
            {
                LoaddgvAccount();
            } else
            {
                dgvAccount.DataSource = db.Accounts.Where(m => m.username.Contains(txtSearch.Text.Trim())).Select(p => new { p.ID, p.username, p.password });
                lbltotal.Text = dgvAccount.RowCount.ToString();
            }

        }

        private void dgvAccount_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }
    }
}
