using System; // Base system types
using System.Linq; // LINQ extensions for queries
using System.Runtime.InteropServices; // For Win32 interop (dragging borderless form)
using System.Windows.Forms; // WinForms UI framework

namespace EmployeeManagementSystem { // Application namespace
    public partial class UpdateSalaryForm : Form { // Form to add/update a salary record

        public const int WM_NCLBUTTONDOWN = 0xA1; // Win32 message code for non-client left button down
        public const int HT_CAPTION = 0x2; // Hit-test value for caption (used to move form)

        [DllImportAttribute("user32.dll")] // Import SendMessage from user32 to simulate dragging
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")] // Import ReleaseCapture to release mouse capture before drag
        public static extern bool ReleaseCapture();

        Salary salary = null; // Entity passed from SalaryForm when editing; null when adding new
        private int? _scode; // optional scode provided instead of full entity to avoid materializing mapped entity
        public UpdateSalaryForm(Salary sal = null) { // Constructor with optional salary entity
            salary = sal; // Store incoming salary entity
            InitializeComponent(); // Initialize designer components
        }
        public UpdateSalaryForm(int scode) // Overload: accept scode to avoid materializing Salary entity
        {
            salary = null;
            _scode = scode;
            InitializeComponent();
        }
        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context; uses Properties.Settings.Default.EmployeeManagementSystemConnectionString (from app.config Settings)

        private int CalculateMonths(DateTime from, DateTime to) { // Compute number of months between dates for salary period
            from = from.Date; // Normalize from date
            to = to.Date; // Normalize to date
            if (to < from)
                return 0; // Invalid range
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month); // Raw month diff
            if (to.Day < from.Day)
                months--; // last month incomplete
            if (months < 1)
                months = 1; // treat same/partial month as1
            return months; // Return months
        }

        private long ComputeTotalByPaidDaysLong(int monthlySalary, DateTime from, DateTime to, int unpaidDays) { // Compute net total with unpaid days, returns long to avoid overflow
            var f = from.Date; // Normalize
            var t = to.Date; // Normalize
            if (t < f || monthlySalary <= 0)
                return 0L; // Guard
            int months = CalculateMonths(f, t); // Months span
            if (months <= 0)
                return 0L; // Guard
            long gross = (long)months * (long)monthlySalary; // Gross total
            int rangeDays = (t - f).Days + 1; // Inclusive days in range
            int ud = Math.Min(Math.Max(unpaidDays, 0), Math.Max(rangeDays, 0)); // Clamp unpaid days
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Approx daily rate (ceil)
            decimal deduction = ud * dailyRate; // Deduction by unpaid days
            decimal net = ((decimal)gross) - deduction; // Net
            if (net < 0)
                net = 0m; // No negatives
            if (net > long.MaxValue)
                return long.MaxValue; // Prevent overflow
            return (long)net; // Cast to long
        }

        private void LoadEmployee() { // Populate employee ID and name combo boxes and sync them
            // Bind by Employee ID (property name EmpID) so user can search/type IDs
            cbbEmployee.Items.Clear(); // Clear existing items
            cbbEmployee.DataSource = db.Employees; // Bind to Employees table
            cbbEmployee.DisplayMember = "EmpID"; // Display EmpID
            cbbEmployee.ValueMember = "EmpID"; // Value is EmpID
            cbbEmployee.AutoCompleteMode = AutoCompleteMode.SuggestAppend; // Enable autocomplete
            cbbEmployee.AutoCompleteSource = AutoCompleteSource.ListItems; // Use list items as source

            // Bind fullname list for optional selection
            cbbEmpName.Items.Clear(); // Clear name list
            cbbEmpName.DataSource = db.Employees; // Bind to Employees table
            cbbEmpName.DisplayMember = "EmpName"; // Show name
            cbbEmpName.ValueMember = "EmpID"; // Value is EmpID to sync with ID box
            cbbEmpName.AutoCompleteMode = AutoCompleteMode.SuggestAppend; // Autocomplete
            cbbEmpName.AutoCompleteSource = AutoCompleteSource.ListItems; // Source

            // Sync selections both ways
            cbbEmployee.SelectedIndexChanged -= cbbEmployee_SelectedIndexChanged; // Prevent duplicate handler attach
            cbbEmpName.SelectedIndexChanged -= cbbEmpName_SelectedIndexChanged; // Prevent duplicate handler attach
            cbbEmployee.SelectedIndexChanged += cbbEmployee_SelectedIndexChanged; // Add sync handler
            cbbEmpName.SelectedIndexChanged += cbbEmpName_SelectedIndexChanged; // Add sync handler
        }

        private void ReloadSalaryFromDb() { // Refresh 'salary' reference with fresh DB data
            if (salary == null)
                return; // Nothing to reload
            // Use local context to get fresh values (avoid stale entity from other DataContext)
            var fresh = db.Salaries.SingleOrDefault(s => s.Scode == salary.Scode); // Query by primary key
            if (fresh != null)
                salary = fresh; // replace reference
        }

        private void cbbEmployee_SelectedIndexChanged(object sender, EventArgs e) { // Sync name combo when ID selection changes and prefill date range
            if (cbbEmployee.SelectedValue != null && cbbEmpName.ValueMember == "EmpID") // Ensure valid selection and mapping
            {
                cbbEmpName.SelectedValue = cbbEmployee.SelectedValue; // Sync name selection

                // Prefill the next suggested period: start at the last To (half-open policy: previous [From, To) => next starts at previous To)
                try // Safe prefill attempt
                {
                    int empId = Convert.ToInt32(cbbEmployee.SelectedValue); // Selected employee ID
                    var last = db.Salaries
                        .Where(s => s.EmployeeID == empId) // Salaries for this employee
                        .OrderByDescending(s => s.To) // Most recent by 'To'
                        .FirstOrDefault(); // Take latest
                    if (last != null)
                    {
                        var suggestedFrom = last.To.Date; // half-open implies previous To is first day of new range
                        if (dtpkFrom.Value.Date != suggestedFrom)
                            dtpkFrom.Value = suggestedFrom; // Set From
                        if (dtpkTo.Value.Date <= suggestedFrom)
                            dtpkTo.Value = suggestedFrom.AddMonths(1); // Set To at least one month later
                    }
                } catch { /* ignore prefill issues */ } // Ignore minor prefill errors
            }
        }
        private void cbbEmpName_SelectedIndexChanged(object sender, EventArgs e) { // Sync ID combo when name selection changes
            if (cbbEmpName.SelectedValue != null && cbbEmployee.ValueMember == "EmpID") // Ensure mapping ok
            {
                cbbEmployee.SelectedValue = cbbEmpName.SelectedValue; // Sync ID selection
            }
        }

        private void ptbClose_Click(object sender, EventArgs e) { // Close button click
            this.Dispose(); // Close form
        }

        private void UpdateSalaryForm_Load(object sender, EventArgs e) { // Form Load event
            LoadEmployee(); // Populate combos
            // If we have a Salary entity provided, populate UI normally
            if (salary != null) // If editing existing record
            {
                txtSalary.Text = CurrencyFormatter.Format(salary.Salary1); // Prefill salary with formatted value
                try // Try to set selected employee in both combos
                {
                    cbbEmployee.SelectedValue = salary.EmployeeID; // Select by ID
                    cbbEmpName.SelectedValue = salary.EmployeeID; // Select matching name
                } catch { } // Ignore selection issues
                dtpkFrom.Value = salary.From; // Set From date
                dtpkTo.Value = salary.To; // Set To date
                dtpkPayDate.Value = salary.Paydate; // Set Pay date
                numUnpaidDays.Value = salary.UnpaidDays; // Set unpaid days
            }
            else if (_scode.HasValue) // If caller passed only scode (avoid materializing entire mapped entity)
            {
                var row = db.Salaries
                            .Where(s => s.Scode == _scode.Value)
                            .Select(s => new { s.Salary1, s.From, s.To, s.Paydate, s.UnpaidDays, s.EmployeeID })
                            .FirstOrDefault();
                if (row != null)
                {
                    txtSalary.Text = CurrencyFormatter.Format(row.Salary1);
                    try { cbbEmployee.SelectedValue = row.EmployeeID; cbbEmpName.SelectedValue = row.EmployeeID; } catch { }
                    dtpkFrom.Value = row.From;
                    dtpkTo.Value = row.To;
                    dtpkPayDate.Value = row.Paydate;
                    numUnpaidDays.Value = row.UnpaidDays;
                    // set internal salary reference minimal info to allow Update to know Scode
                    salary = new Salary(); // dummy container with only Scode used later
                    salary.Scode = _scode.Value;
                }
            }
        }

        // Half-open overlap check: [From, To) — allows boundary equality (adjacent periods). Avoid Nullable.Value use in query.
        private bool HasOverlappingSalary(int empId, DateTime from, DateTime to, int? excludeScode = null) { // Detect overlap with existing salary ranges
            var f = from.Date; // Normalize from
            var t = to.Date; // Normalize to
            // Build query without referencing excludeScode.Value inside expression when null.
            var q = db.Salaries.Where(s => s.EmployeeID == empId); // Base query for employee
            if (excludeScode.HasValue) // If an existing record should be excluded (for update)
            {
                int ex = excludeScode.Value; // safe extraction
                q = q.Where(s => s.Scode != ex); // Exclude that record
            }
            // Execute server-side simple filter, then evaluate overlap in memory to keep logic clear and avoid provider issues.
            foreach (var s in q) // Iterate matching salaries
            {
                if (f < s.To && s.From < t) // Half-open overlap condition
                    return true; // overlap
            }
            return false; // No overlap
        }

        private void btnAdd_Click(object sender, EventArgs e) { // Add button handler
            if (string.IsNullOrWhiteSpace(cbbEmployee.Text) || string.IsNullOrWhiteSpace(txtSalary.Text)) // Basic validation
            {
                MessageBox.Show("Miss data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform user
                return; // Stop
            }
            if (cbbEmployee.SelectedValue == null) // Ensure employee selected
            {
                MessageBox.Show("Select an employee", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform user
                return; // Stop
            }
            int empId = Convert.ToInt32(cbbEmployee.SelectedValue); // Get employee ID
            DateTime from = dtpkFrom.Value.Date; // Read From date
            DateTime to = dtpkTo.Value.Date; // Read To date
            if (from >= to) // Validate order
            {
                MessageBox.Show("'From' date must be strictly before 'To' date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Warn
                return; // Stop
            }
            if (HasOverlappingSalary(empId, from, to, null)) // Prevent period overlap
            {
                MessageBox.Show("Salary period overlaps an existing record for this employee.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Warn
                return; // Stop
            }
            var epl = db.Employees.SingleOrDefault(emp => emp.EmpID == empId); // Load employee info
            var sal = new Salary // Create new salary entity
            {
                EmployeeID = empId, // Set employee ID
                EmployeeName = epl?.EmpName ?? string.Empty, // Denormalize name for report
                From = from, // Period start
                To = to, // Period end
                Period = CalculateMonths(from, to), // Months count
                Paydate = dtpkPayDate.Value.Date, // Pay date
                UnpaidDays = (int)numUnpaidDays.Value // Unpaid days
            };
            int parsedSal; // Temp parsed salary
            sal.Salary1 = int.TryParse(txtSalary.Text, out parsedSal) ? parsedSal : (epl?.EmpSal ?? 0); // Use input or default to employee base salary
            var netAdd = ComputeTotalByPaidDaysLong(sal.Salary1, sal.From, sal.To, sal.UnpaidDays); // Compute net total
            sal.totalsal = (int)Math.Min(netAdd, (long)int.MaxValue); // Clamp to int for storage
            db.Salaries.InsertOnSubmit(sal); // Queue insert
            db.SubmitChanges(); // Commit to database
            MessageBox.Show("Inserted Sucessfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Success message
            this.Dispose(); // Close form
        }

        private void btnUpdate_Click(object sender, EventArgs e) { // Update button handler
            // Use parameterized UPDATE to avoid materializing mapped Salary entity that may cause InvalidCastException
            // Determine scode from either salary object or _scode
            int scodeToUpdate = salary != null ? salary.Scode : (_scode.HasValue ? _scode.Value : -1);
            if (scodeToUpdate <= 0)
                return; // Nothing to update
            if (cbbEmployee.SelectedValue == null) // Validate selection
            {
                MessageBox.Show("Select an employee", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform
                return; // Stop
            }
            int empId = Convert.ToInt32(cbbEmployee.SelectedValue); // Selected employee ID
            DateTime from = dtpkFrom.Value.Date; // From date
            DateTime to = dtpkTo.Value.Date; // To date
            if (from >= to) // Validate
            {
                MessageBox.Show("'From' date must be strictly before 'To' date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Warn
                return; // Stop
            }
            if (HasOverlappingSalary(empId, from, to, scodeToUpdate)) // Check overlap excluding this record
            {
                MessageBox.Show("Updated period overlaps another salary record for this employee.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Warn
                return; // Stop
            }

            int parsedSalary = CurrencyFormatter.ParseToInt(txtSalary.Text);
            if (parsedSalary <= 0)
            {
                var epl = db.Employees.SingleOrDefault(emp => emp.EmpID == empId);
                if (epl != null) parsedSalary = epl.EmpSal;
            }
            var epl2 = db.Employees.SingleOrDefault(emp => emp.EmpID == empId);
            string empName = epl2?.EmpName ?? string.Empty;
            int period = CalculateMonths(from, to);
            int unpaidDays = (int)numUnpaidDays.Value;
            long netUpd = ComputeTotalByPaidDaysLong(parsedSalary, from, to, unpaidDays);
            int clamped = (int)Math.Min(netUpd, (long)int.MaxValue);

            // Perform parameterized UPDATE
            db.ExecuteCommand("UPDATE dbo.[Salary] SET EmployeeID = {0}, Salary = {1}, EmployeeName = {2}, [From] = {3}, [To] = {4}, Paydate = {5}, Period = {6}, UnpaidDays = {7}, totalsal = {8} WHERE Scode = {9}",
                empId, parsedSalary, empName, from, to, dtpkPayDate.Value.Date, period, unpaidDays, clamped, scodeToUpdate);

            MessageBox.Show("Updated Sucessfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform success
            this.Dispose(); // Close form
        }

        private void pnltop_MouseDown(object sender, MouseEventArgs e) { // Allow dragging form by top panel
            if (e.Button == MouseButtons.Left) // Only left click
            {
                ReleaseCapture(); // Release mouse capture
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); // Send message to move window
            }
        }
        private void label4_Click(object sender, EventArgs e) { // Unused label click handler
        }
        private void dtpkPayDate_ValueChanged(object sender, EventArgs e) { // Pay date change handler (unused)
        }
    }
}
