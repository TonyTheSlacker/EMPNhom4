using System; // Base types
using System.Data; // Data-related types (DataTable etc.)
using System.Drawing; // For Image, Bitmap, Color
using System.IO; // File operations (image copy/delete)
using System.Linq; // LINQ queries
using System.Windows.Forms; // WinForms UI components

namespace EmployeeManagementSystem { // Application namespace
    public partial class EmployeeForm : Form { // Employee management form definition

        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context (connects using Properties.Settings.Default.EmployeeManagementSystemConnectionString)
        // Add a list to hold all employees for in-memory filtering
        private IQueryable<Employee> allEmployees; // Cached queryable employees for search filtering without hitting DB each keystroke
        public EmployeeForm() { // Constructor
            InitializeComponent(); // Initialize designer-created controls
        }

        private void ResetContext() { // Recreate data context to ensure fresh DB state
            try
            {
                db.Dispose(); // Dispose existing context (releases connection)
            } catch { } // Ignore dispose exceptions
            db = new EmployeeDataContext(); // New context instance (reloads mapping & uses settings connection string)
        }

        private void ApplyTotals() { // Update total salary UI
            var total = db.Employees.Any() ? db.Employees.Sum(e => e.EmpSal) : 0; // Sum employee salaries (0 if none)
            labelTotalSalaryTitle.Visible = true; // Ensure title label visible
            lblTotalSalary.Visible = true; // Ensure value label visible
            lblTotalSalary.Text = CurrencyFormatter.Format(total) + " $"; // Display formatted total
        }

        public void LoaddgvEmployee() { // Load employees into grid from database
            ResetContext(); // Refresh context
            // Load all employees into the in-memory list
            allEmployees = db.Employees.AsQueryable(); // Obtain queryable table for deferred execution
            BindGrid(allEmployees); // Bind full set
        }

        // New method to bind a list of employees to the grid
        private void BindGrid(IQueryable<Employee> employees) { // Populate DataGridView from projected employee data
            dgvEmployee.RowTemplate.Height = 50; // Set row height (for images)
            string t = Application.StartupPath + @"\AddressImage\"; // Folder path for employee images
            var dataSource = employees.Select(p => new // Project entity to anonymous type for grid binding
            {
                p.EmpID, // Employee ID
                p.EmpGen, // Gender
                DepName = p.Department.DepName, // Department name via navigation property
                p.EmpName, // Name
                p.EmpDOB, // Date of birth
                p.EmpJDate, // Join date
                EmpSal = CurrencyFormatter.Format(p.EmpSal), // Formatted salary string
                EmpImage = Image.FromFile(t + p.EmpImage.ToString()) // Load image file into Image object
            }).ToList(); // Execute query and materialize list

            dgvEmployee.DataSource = dataSource; // Bind list to grid
            lbltotal.Text = dgvEmployee.RowCount.ToString(); // Update total rows label
            dgvEmployee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Auto-size columns to fill width
            ApplyTotals(); // Update total salary display
        }

        private void EmployeeForm_Load(object sender, EventArgs e) { // Form load handler
            LoaddgvEmployee(); // Initial load from DB
        }

        private void dgvEmployee_CellClick(object sender, DataGridViewCellEventArgs e) { // Handle clicks on image/edit/delete columns
            int posrow = e.RowIndex; // Row index clicked
            int poscol = e.ColumnIndex; // Column index clicked
            if (posrow >= 0 && poscol >= 0) // Validate row/column indices
            {
                if (dgvEmployee.Columns[poscol].Name == "Delete") // Delete operation
                {
                    if (MessageBox.Show("Are you sure delete this Employee ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) // Confirm deletion
                    {
                        Salary sal = db.Salaries.SingleOrDefault(m => m.EmployeeID == int.Parse(dgvEmployee["ID", posrow].Value.ToString())); // Load related salary (if any)
                        Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString())); // Load employee entity
                        if (sal != null)
                            db.Salaries.DeleteOnSubmit(sal); // Mark salary for deletion
                        if (epl != null)
                            db.Employees.DeleteOnSubmit(epl); // Mark employee for deletion
                        db.SubmitChanges(); // Persist deletions to DB
                    }
                    LoaddgvEmployee(); // Refresh grid
                } else if (dgvEmployee.Columns[poscol].Name == "Edit") // Edit operation
                {
                    Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString())); // Load employee for editing
                    UpdateEmployeeForm f = new UpdateEmployeeForm(epl); // Instantiate edit form with entity
                    // Pass the exact salary text shown in the grid so the popup matches what user sees
                    var salaryCellText = dgvEmployee["Column6", posrow].Value != null ? dgvEmployee["Column6", posrow].Value.ToString() : string.Empty; // Get formatted salary cell text
                    f.PrefilledSalaryText = salaryCellText; // Provide salary string to form
                    f.btnAdd.Enabled = false; // Disable add (editing mode)
                    f.btnUpdate.Enabled = true; // Enable update button
                    f.ShowDialog(); // Show modal edit form
                    LoaddgvEmployee(); // Refresh after update
                } else if (dgvEmployee.Columns[poscol].Name == "imagecol") // Change image operation
                {
                    string addressimage = ""; // Selected image full path
                    string namefile = ""; // Selected file name
                    OpenFileDialog dal = new OpenFileDialog(); // File dialog for image selection
                    dal.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp;)|*.jpg; *.jpeg; *.gif; *.bmp;"; // Allow common image formats
                    if (dal.ShowDialog() == DialogResult.OK) // If user picked a file
                    {
                        addressimage = dal.FileName; // Capture full path
                        namefile = Path.GetFileName(dal.FileName); // Capture file name only
                        Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString())); // Load employee
                        if (epl != null)
                        {
                            if (epl.EmpImage == namefile) // No change (same file name)
                            {
                                LoaddgvEmployee(); // Refresh (ensures UI updates)
                                return; // Exit early
                            }
                            if (epl.EmpImage != namefile && File.Exists(Application.StartupPath + "\\AddressImage\\" + namefile)) // File already exists in target path
                            {
                                epl.EmpImage = namefile; // Update image field
                                db.SubmitChanges(); // Save DB changes
                                LoaddgvEmployee(); // Refresh grid
                                return; // Done
                            }
                            if (File.Exists(Application.StartupPath + "\\AddressImage\\" + namefile)) // Remove existing conflicting file if needed
                            {
                                File.Delete(Application.StartupPath + "\\AddressImage\\" + namefile); // Delete old file
                            }
                            File.Copy(dal.FileName, Application.StartupPath + "\\AddressImage\\" + namefile); // Copy new image into app folder
                            epl.EmpImage = namefile; // Update employee record
                            db.SubmitChanges(); // Persist change
                        }
                    }
                    LoaddgvEmployee(); // Refresh after image update
                }
            }
        }

        private void ptbAdd_Click(object sender, EventArgs e) { // Add new employee button/picture click
            var f = new UpdateEmployeeForm(); // Instantiate add form
            f.btnAdd.Enabled = true; // Enable add functionality
            f.btnUpdate.Enabled = false; // Disable update functionality
            f.ShowDialog(); // Show modal form
            LoaddgvEmployee(); // Refresh grid after potential insert
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) { // Search textbox change handler
            string searchValue = txtSearch.Text.Trim().ToLower(); // Normalize search input

            if (string.IsNullOrEmpty(searchValue)) // No search criteria
            {
                // If search is cleared, show all employees from the in-memory list
                BindGrid(allEmployees); // Rebind full list (no DB query; already cached)
                return; // Exit handler
            }

            // Use a comprehensive LINQ query to filter the in-memory list
            var filteredEmployees = allEmployees // Operate on in-memory queryable
                .Where(emp => emp.EmpName.ToLower().Contains(searchValue) || // Match on name
                              emp.EmpID.ToString().Contains(searchValue) || // Match on ID
                              emp.EmpGen.ToLower().Contains(searchValue) || // Match on gender
                              emp.Department.DepName.ToLower().Contains(searchValue)); // Match on department name

            BindGrid(filteredEmployees); // Bind filtered results
        }
    }
}
