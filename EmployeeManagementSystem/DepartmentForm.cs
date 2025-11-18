using System; // Base types
using System.Data; // Data-related types (not directly used here but common for DataGridView bindings)
using System.Linq; // LINQ query operators
using System.Windows.Forms; // WinForms UI types

namespace EmployeeManagementSystem { // Application namespace
    public partial class DepartmentForm : Form { // Department management form
        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context (uses connection string from settings)
        public DepartmentForm() { // Constructor
            InitializeComponent(); // Initialize designer-created controls
        }
        public void LoaddgvDepartment() { // Populate department grid view
            dgvDepartment.DataSource = db.Departments.Select(d => new { d.DepId, d.DepName }); // Query Departments table and bind projection
            lbltotal.Text = dgvDepartment.RowCount.ToString(); // Update total count label
        }
        private void DepartmentForm_Load(object sender, EventArgs e) { // Form load event handler
            LoaddgvDepartment(); // Load departments when form opens

        }

        private void dgvDepartment_CellClick(object sender, DataGridViewCellEventArgs e) { // Handle clicks on department grid cells
            int posrow = e.RowIndex; // Clicked row index
            int poscol = e.ColumnIndex; // Clicked column index
            if (posrow >= 0 && poscol >= 0) // Ensure click is on a valid cell
            {
                if (poscol == 1) // Delete action column (assuming column 1 is delete button/icon)
                {
                    if (MessageBox.Show("Are you sure delete this Department ? If you delete this department , employee use this department will be deleted .", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) // Confirm deletion
                    {
                        Department dep = db.Departments.SingleOrDefault(m => m.DepId == int.Parse(dgvDepartment[2, posrow].Value.ToString())); // Find department by id from hidden cell index 2
                        db.Salaries.DeleteAllOnSubmit(db.Salaries.Where(m => m.EmployeeID == m.Employee.EmpID && m.Employee.EmpDep == dep.DepId)); // Remove salaries of employees in this department
                        db.Employees.DeleteAllOnSubmit(db.Employees.Where(m => m.EmpDep == dep.DepId)); // Remove employees in this department

                        db.Departments.DeleteOnSubmit(dep); // Mark department for deletion
                        db.SubmitChanges(); // Commit all deletions to database
                        LoaddgvDepartment(); // Refresh grid after deletion
                    }

                } else if (poscol == 0) // Update action column (assuming column 0 is edit button/icon)
                {
                    Department dep = db.Departments.SingleOrDefault(m => m.DepId == int.Parse(dgvDepartment[2, posrow].Value.ToString())); // Load department entity to edit
                    UpdateDepartmentForm f = new UpdateDepartmentForm(dep); // Create update form with existing department
                    f.btnAdd.Enabled = false; // Disable add button (edit mode)
                    f.btnUpdate.Enabled = true; // Enable update button
                    f.ShowDialog(); // Show modal dialog
                    LoaddgvDepartment(); // Reload departments after potential update
                }

            }
        }

        private void ptbAdd_Click(object sender, EventArgs e) { // Add new department picture box click
            UpdateDepartmentForm f = new UpdateDepartmentForm(); // Create form for new department
            f.btnAdd.Enabled = true; // Enable add button
            f.btnUpdate.Enabled = false; // Disable update button
            f.ShowDialog(); // Show modal dialog
            LoaddgvDepartment(); // Refresh list after adding
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) { // Search textbox text change handler
            if (txtSearch.Text == "") // If search cleared
            {
                LoaddgvDepartment(); // Reload full list
            } else // Filter case
            {
                dgvDepartment.DataSource = db.Departments.Where(m => m.DepName.Contains(txtSearch.Text.Trim())).Select(p => new { p.DepId, p.DepName }); // Filter by name contains search text
                lbltotal.Text = dgvDepartment.RowCount.ToString(); // Update count
            }

        }

        private void dgvDepartment_CellContentClick(object sender, DataGridViewCellEventArgs e) { // Unused cell content click handler (placeholder)

        }

        /* Legacy CRUD handlers (commented out) previously used direct controls for add/update/delete instead of action columns
        private void dgvDepartment_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int vitri = e.RowIndex;
            if(vitri>=0&& dgvDepartment[1, vitri].Value!=null)
            {
                txtDepartmentName.Text = dgvDepartment[1, vitri].Value.ToString();
            }    
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtDepartmentName.Text.Trim();
            if (name != "")
            {
                Department dep = new Department();
                dep.DepName = name;
                db.Departments.InsertOnSubmit(dep);
                db.SubmitChanges();
                dgvDepartment.DataSource = db.Departments;
                dgvDepartment.Refresh();
                txtDepartmentName.ResetText();
                MessageBox.Show("Insert Successfully");
            }
            else
            {
                MessageBox.Show("Miss Data");
            } 
                
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string name = txtDepartmentName.Text.Trim();
            int vitri = dgvDepartment.CurrentRow.Index;
            if(name !="" &&vitri>=0 && dgvDepartment[0,vitri].Value!=null )
            {
                int IDpart = int.Parse(dgvDepartment[0, vitri].Value.ToString());
                Department dep = db.Departments.SingleOrDefault(p => p.DepId == IDpart);
                if(dep!=null)
                {
                    dep.DepName=name;
                    db.SubmitChanges();
                    txtDepartmentName.ResetText();
                    MessageBox.Show("Update Successfully");
                    LoaddgvDepartment();

                }    
            }    
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int vitri = dgvDepartment.CurrentRow.Index;
            if ( vitri >= 0 && dgvDepartment[0, vitri].Value != null)
            {
                int IDpart = int.Parse(dgvDepartment[0, vitri].Value.ToString());
                Department dep = db.Departments.SingleOrDefault(p => p.DepId == IDpart);
                if (dep != null)
                {
                    db.Departments.DeleteOnSubmit(dep);
                    db.SubmitChanges();
                    txtDepartmentName.ResetText();
                    MessageBox.Show("Delete Successfully");
                    LoaddgvDepartment();

                }
            }
        }*/
    }
}
