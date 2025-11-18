using System; // Base types and events
using System.Linq; // LINQ extensions for queries
using System.Runtime.InteropServices; // Win32 interop attributes for dragging borderless form
using System.Windows.Forms; // WinForms UI framework

namespace EmployeeManagementSystem { // Application namespace
    public partial class UpdateEmployeeForm : Form { // Form used to add/update an employee
        public const int WM_NCLBUTTONDOWN = 0xA1; // Win32 message for non-client left button down (dragging)
        public const int HT_CAPTION = 0x2; // Hit-test caption area

        [DllImportAttribute("user32.dll")] // Import SendMessage (used for dragging window)
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")] // Import ReleaseCapture (used before dragging)
        public static extern bool ReleaseCapture();

        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL context; uses Properties.Settings.Default.EmployeeManagementSystemConnectionString from app.config
        Employee employee = null; // Currently edited employee (null when adding)

        // Allow caller to pass the exact salary text from the grid (already formatted)
        public string PrefilledSalaryText { // Optional prefilled salary text (formatted)
            get; set;
        }

        public UpdateEmployeeForm(Employee e = null) { // Constructor accepts existing Employee for edit
            employee = e; // Store employee reference (may be null)
            InitializeComponent(); // Initialize UI components

        }
        void LoadDataDep() { // Populate department combobox from database
            cbbDepartment.Items.Clear(); // Clear current items
            cbbDepartment.DataSource = db.Departments; // Bind to Departments table
            cbbDepartment.DisplayMember = "DepName"; // Show department name
            cbbDepartment.ValueMember = "DepId"; // Value is department ID
        }
        private void ptbClose_Click(object sender, EventArgs e) { // Close button click
            this.Dispose(); // Close form
        }

        private void pnltop_MouseDown(object sender, MouseEventArgs e) { // Allow form dragging via top panel
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture(); // Release mouse capture
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); // Send move message
            }
        }

        private void btnAdd_Click(object sender, EventArgs e) { // Add new employee button
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || cbbgender.Text == "") // Validate fields
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Show validation message
                return; // Abort
            } else
            {
                Employee epl = new Employee(); // Create new employee entity
                epl.EmpName = txtName.Text.Trim(); // Set name
                epl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text); // Parse salary from formatted text
                epl.EmpDOB = dtpkDOB.Value; // Date of birth
                epl.EmpJDate = dtpkJDate.Value; // Join date
                epl.EmpGen = cbbgender.SelectedItem.ToString(); // Gender selection
                epl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString()); // Department ID from combo Value
                epl.EmpImage = "avatar.jpg"; // Default avatar path
                db.Employees.InsertOnSubmit(epl); // Queue insert in context
                db.SubmitChanges(); // Persist to DB
                MessageBox.Show("Added sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Feedback
                this.Dispose(); // Close form
            }
        }

        // Helper logic copied from SalaryForm to keep salary rows consistent when employee base salary changes
        private int CalcMonths(DateTime from, DateTime to) { // Compute months between two dates
            from = from.Date; // Normalize
            to = to.Date; // Normalize
            if (to < from)
                return 0; // Invalid range
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month); // Raw month diff
            if (to.Day < from.Day)
                months--; // Adjust for incomplete final month
            if (months < 1)
                months = 1; // Minimum 1 month
            return months; // Result
        }
        private int ComputeNetTotal(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) { // Compute net salary total (int)
            var f = from.Date; // Normalize
            var t = to.Date; // Normalize
            if (t < f || monthlySalary <= 0)
                return 0; // Guard
            int months = CalcMonths(f, t); // Months span
            if (months <= 0)
                return 0; // Guard
            decimal gross = months * (decimal)monthlySalary; // Gross amount
            int rangeDays = (t - f).Days + 1; // Total days inclusive
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0)); // Clamp unpaid days
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Daily rate approx
            decimal net = gross - (unpaidDays * dailyRate); // Net total
            if (net < 0)
                net = 0; // No negatives
            return (int)net; // Cast to int
        }

        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) { // Compute net salary total (long)
            var f = from.Date; // Normalize
            var t = to.Date; // Normalize
            if (t < f || monthlySalary <= 0)
                return 0L; // Guard
            int months = CalcMonths(f, t); // Months span
            if (months <= 0)
                return 0L; // Guard
            long gross = (long)months * monthlySalary; // Gross as long
            int rangeDays = (t - f).Days + 1; // Inclusive day count
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0)); // Clamp unpaid days
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Daily rate
            decimal netDec = (decimal)gross - (unpaidDays * dailyRate); // Net as decimal
            if (netDec < 0)
                netDec = 0m; // No negatives
            if (netDec > long.MaxValue)
                return long.MaxValue; // Prevent overflow
            return (long)netDec; // Cast to long
        }

        private void txtSalary_KeyPress(object sender, KeyPressEventArgs e) { // Restrict salary input to digits and control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Block invalid char
            }
        }

        private void UpdateEmployeeForm_Load(object sender, EventArgs e) { // Form Load event handler
            LoadDataDep(); // Fill department combo from DB
            if (employee != null) // If editing existing employee
            {
                txtName.Text = employee.EmpName; // Prefill name
                // Prefer the text shown in the grid if provided (ensures exact prefill like400.000)
                if (!string.IsNullOrWhiteSpace(PrefilledSalaryText))
                    txtSalary.Text = PrefilledSalaryText; // Use provided formatted text
                else
                    txtSalary.Text = CurrencyFormatter.Format(employee.EmpSal); // Format base salary
                cbbgender.Text = employee.EmpGen.ToString(); // Prefill gender
                // Select department by value to ensure SelectedValue is valid later
                try
                {
                    cbbDepartment.SelectedValue = employee.EmpDep; // Select department by ID
                } catch { cbbDepartment.Text = employee.Department.DepName.ToString().Trim(); } // Fallback by text
                // Set DateTimePicker Value instead of Text
                dtpkDOB.Value = employee.EmpDOB; // Prefill DOB
                dtpkJDate.Value = employee.EmpJDate; // Prefill Join date
            }
        }

        private void label6_Click(object sender, EventArgs e) { // Unused label click

        }

        private void label1_Click(object sender, EventArgs e) { // Unused label click

        }

        private void button1_Click(object sender, EventArgs e) { // Update existing employee button
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || string.IsNullOrEmpty(cbbgender.Text) || string.IsNullOrEmpty(cbbDepartment.Text)) // Validate inputs
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Feedback
            } else
            {
                Employee empl = db.Employees.SingleOrDefault(m => m.EmpID == employee.EmpID); // Load the employee entity by ID
                empl.EmpName = txtName.Text.Trim(); // Update name
                empl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text); // Update base salary
                empl.EmpDOB = dtpkDOB.Value; // Update DOB
                empl.EmpJDate = dtpkJDate.Value; // Update join date
                empl.EmpGen = cbbgender.SelectedItem.ToString(); // Update gender
                empl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString()); // Update department ID
                // Propagate new base salary to existing salary records (business choice)
                // Avoid materializing whole Salary entities (which can fail if DB column type doesn't match the LINQ mapping).
                // Instead project minimal columns and perform a parameterized UPDATE for each row to avoid InvalidCastExceptions.
                var rows = db.Salaries
                             .Where(s => s.EmployeeID == empl.EmpID)
                             .Select(s => new { s.Scode, s.From, s.To, s.UnpaidDays })
                             .ToList();
                foreach (var r in rows)
                {
                    DateTime newFrom = empl.EmpJDate; // align period start with employee join date
                    int newPeriod = CalcMonths(newFrom, r.To);
                    long newTotal = ComputeNetTotalLong(empl.EmpSal, newFrom, r.To, r.UnpaidDays);
                    int clamped = (int)Math.Min(newTotal, (long)int.MaxValue);
                    // Update row by primary key; use parameterized ExecuteCommand to avoid loading the mapped totalsal property
                    db.ExecuteCommand("UPDATE dbo.[Salary] SET Salary = {0}, [From] = {1}, Period = {2}, totalsal = {3} WHERE Scode = {4}", empl.EmpSal, newFrom, newPeriod, clamped, r.Scode);
                }

                db.SubmitChanges(); // Save changes to DB (commits any pending changes to other tracked entities)
                MessageBox.Show("Updated sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Success feedback
                this.Dispose(); // Close form
            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e) { // Add (duplicate button) handler
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || cbbgender.Text == "") // Validate
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Feedback
                return; // Abort
            } else
            {
                Employee epl = new Employee(); // Create new employee
                epl.EmpName = txtName.Text.Trim(); // Name
                epl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text); // Base salary
                epl.EmpDOB = dtpkDOB.Value; // DOB
                epl.EmpJDate = dtpkJDate.Value; // Join date
                epl.EmpGen = cbbgender.SelectedItem.ToString(); // Gender
                epl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString()); // Department ID
                epl.EmpImage = "avatar.jpg"; // Default image
                db.Employees.InsertOnSubmit(epl); // Queue insert
                db.SubmitChanges(); // Save
                MessageBox.Show("Added sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Success
                this.Dispose(); // Close
            }
        }
    }
}
