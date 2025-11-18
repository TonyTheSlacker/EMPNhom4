using System; // Basic types and events
using System.Data; // Data-related types (not used heavily here)
using System.Linq; // LINQ extension methods for queries
using System.Windows.Forms; // WinForms UI classes

namespace EmployeeManagementSystem {
    public partial class AccountForm : Form {
        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context; connection string comes from Properties.Settings.Default.EmployeeManagementSystemConnectionString (app.config)
        public AccountForm() {
            InitializeComponent(); // Initialize designer-created controls
        }

        public void LoaddgvAccount() {
            // Bind DataGridView to a projection of Accounts table (ID, username, password, Email)
            dgvAccount.DataSource = db.Accounts.Select(a => new { a.ID, a.username, a.password, a.Email });
            lbltotal.Text = dgvAccount.RowCount.ToString(); // Show total row count
        }
        private void AccountForm_Load(object sender, EventArgs e) {
            LoaddgvAccount(); // Load accounts when form loads
        }

        private void ptbAdd_Click(object sender, EventArgs e) {
            UpdateAccountForm f = new UpdateAccountForm(); // Open form to add a new account
            f.ShowDialog();
            LoaddgvAccount(); // Refresh grid after potential add
        }

        private void dgvAccount_CellClick(object sender, DataGridViewCellEventArgs e) {
            int posrow = e.RowIndex; // Clicked row index
            int poscol = e.ColumnIndex; // Clicked column index
            if (posrow >= 0 && poscol >= 0)
            {
                if (dgvAccount.Columns[poscol].Name == "Delete")
                {
                    if (MessageBox.Show("Are you sure delete this Account ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {

                        // Load the Account entity by ID (reading ID from the grid cell named "IDAcc")
                        Account acc = db.Accounts.SingleOrDefault(m => m.ID == int.Parse(dgvAccount["IDAcc", posrow].Value.ToString()));
                        db.Accounts.DeleteOnSubmit(acc); // Mark for deletion

                        db.SubmitChanges(); // Persist deletion to the database
                        LoaddgvAccount(); // Refresh grid to reflect deletion
                    }

                } else if (dgvAccount.Columns[poscol].Name == "Edit")
                {
                    // Load the Account entity by ID and open edit dialog
                    Account acc = db.Accounts.SingleOrDefault(m => m.ID == int.Parse(dgvAccount["IDAcc", posrow].Value.ToString()));
                    UpdateAccountForm f = new UpdateAccountForm(acc);
                    f.btnAdd.Enabled = false; // Disable Add in edit mode
                    f.btnUpdate.Enabled = true; // Enable Update
                    f.ShowDialog(); // Show modal
                    LoaddgvAccount(); // Refresh grid after possible update
                }

            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) {
            if (txtSearch.Text == "")
            {
                LoaddgvAccount(); // If search box cleared, reload all accounts
            } else
            {
                // Filter Accounts by username containing the search term and bind to grid
                dgvAccount.DataSource = db.Accounts.Where(m => m.username.Contains(txtSearch.Text.Trim())).Select(p => new { p.ID, p.username, p.password });
                lbltotal.Text = dgvAccount.RowCount.ToString(); // Update total
            }

        }

        private void dgvAccount_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }
    }
}
