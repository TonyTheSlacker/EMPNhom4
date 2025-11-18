using System; // Base types and events
using System.Data; // Data-related types (for DataTable etc.)
using System.Linq; // LINQ operators
using System.Windows.Forms; // WinForms UI controls
using System.Data.Linq; // added for RefreshMode (LINQ-to-SQL context refresh)

namespace EmployeeManagementSystem { // App namespace
    public partial class SalaryForm : Form { // Form for viewing/managing salaries
        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL context; reads connection from Properties.Settings.Default.EmployeeManagementSystemConnectionString (app.config)
        public SalaryForm() { // Constructor
            InitializeComponent(); // Initialize designer components
        }

        private void ResetContext() { // recreate context to avoid stale tracked entities
            try { db.Dispose(); } catch { } // Dispose current context defensively
            db = new EmployeeDataContext(); // New context (uses same Settings-based connection string)
        }
        private void RefreshTrackedRows() { // fallback if not recreating
            try { db.Refresh(RefreshMode.OverwriteCurrentValues, db.Salaries); } catch { } // Refresh tracked salary rows from DB
        }

        private int CalcMonths(DateTime from, DateTime to) { // Calculate number of salary months between dates
            from = from.Date; // Normalize from date (strip time)
            to = to.Date; // Normalize to date
            if (to < from)
                return 0; // Invalid range -> 0
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month); // Raw month difference
            if (to.Day < from.Day)
                months--; // incomplete last month -> subtract one
            if (months < 1)
                months = 1; // minimum 1
            return months; // Return months
        }

        // Long version to avoid overflow (support big totals) -------------------------------------------------
        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) { // Compute net total salary as long
            var f = from.Date; // From date (date-only)
            var t = to.Date; // To date (date-only)
            if (t < f || monthlySalary <= 0)
                return 0L; // Guard invalid input
            int months = CalcMonths(f, t); // Months span
            if (months <= 0)
                return 0L; // Guard
            // Use long for gross
            long gross = (long)months * (long)monthlySalary; // Gross total = months * monthly salary
            int rangeDays = (t - f).Days + 1; // Inclusive number of days in range
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0)); // Clamp unpaid days
            // Use decimal daily rate but convert to long after multiplication
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Approx daily rate (ceil)
            decimal deduction = unpaidDays * dailyRate; // Deduction amount
            decimal netDec = ((decimal)gross) - deduction; // Net = gross - deduction
            if (netDec < 0)
                netDec = 0m; // No negatives
            // Clamp to long range
            if (netDec > long.MaxValue)
                return long.MaxValue; // Prevent overflow
            return (long)netDec; // Cast to long
        }

        // Legacy int version - now wraps long version and clamps to int for storage --------------------------
        private int ComputeNetTotal(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) { // int wrapper for net total
            long v = ComputeNetTotalLong(monthlySalary, from, to, unpaidDaysNullable); // Compute as long
            if (v > int.MaxValue)
                return int.MaxValue; // Clamp to int max
            return (int)v; // Return int
        }

        // Bind grid using existing designer columns (AutoGenerateColumns=false)
        public void LoadSalary() { // Load and display salary data into grid
            try
            {
                ResetContext(); // ensure fresh data
                dgvSalary.AutoGenerateColumns = false; // Use predefined columns in designer

                // IMPORTANT: Project only the fields we need to avoid materializing the whole entity
                // which can fail if a mapped column type in DB doesn't match (e.g., totalsal decimal vs int).
                var rows = db.Salaries // Query salaries table via LINQ-to-SQL
                    .Select(p => new { // Select only required columns
                        p.EmployeeID, // Employee ID (denormalized for convenience)
                        p.EmployeeName, // Employee name (denormalized)
                        p.Scode, // Salary code (primary key)
                        p.Salary1, // Monthly salary
                        p.Period, // Stored period months
                        p.From, // From date
                        p.To, // To date
                        p.Paydate, // Pay date
                        p.UnpaidDays // Unpaid days
                    })
                    .ToList(); // Execute and bring into memory

                dgvSalary.DataSource = rows // Transform rows for display
                    .Select(p => new
                    {
                        EmpID = p.EmployeeID, // Grid column: Employee ID
                        EmpName = p.EmployeeName, // Grid column: Employee Name
                        Scode = p.Scode, // Grid column: Salary Code
                        Salary1 = CurrencyFormatter.Format(p.Salary1), // Format monthly salary for display
                        Period = p.Period, // Period
                        From = p.From, // From date
                        To = p.To, // To date
                        Paydate = p.Paydate, // Pay date
                        totalsal = CurrencyFormatter.Format(ComputeNetTotalLong(p.Salary1, p.From, p.To, p.UnpaidDays)), // Computed total salary formatted
                        UnpaidDays = p.UnpaidDays // Unpaid days
                    })
                    .ToList(); // Bindable list

                long totalAll = rows.Sum(s => ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays)); // Sum all computed totals
                lbltotal.Text = CurrencyFormatter.Format(totalAll) + "$"; // Display grand total with currency suffix
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to load salaries: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show error
            }
        }
        private void SalaryForm_Load(object sender, EventArgs e) { // Form Load event

            LoadSalary(); // Initial load of salary data
        }
        private void dgvSalary_CellClick(object sender, DataGridViewCellEventArgs e) { // Grid cell click handler
            int posrow = e.RowIndex; // Clicked row index
            int poscol = e.ColumnIndex; // Clicked column index
            if (posrow >= 0 && poscol >= 0) // Ensure valid data cell
            {
                if (dgvSalary.Columns[poscol].Name == "Delete") // Delete
                {
                    if (MessageBox.Show("Are you sure delete this Salary ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) // Confirm
                    {
                        try
                        {
                            // Avoid materializing the entity (which can throw due to type mismatch). Delete by key directly.
                            int scode = int.Parse(dgvSalary["IDSal", posrow].Value.ToString()); // Read primary key from grid
                            db.ExecuteCommand("DELETE FROM [dbo].[Salary] WHERE [Scode] = {0}", scode); // Execute raw SQL delete using LINQ-to-SQL context
                        } catch (Exception ex)
                        {
                            MessageBox.Show("Delete failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Error feedback
                        }
                        LoadSalary(); // Refresh grid after delete
                    }

                } else if (dgvSalary.Columns[poscol].Name == "Edit") //Update
                {
                    try
                    {
                        int scode = int.Parse(dgvSalary["IDSal", posrow].Value.ToString()); // Read primary key from grid without materializing entity
                        UpdateSalaryForm f = new UpdateSalaryForm(scode) { btnAdd = { Enabled = false }, btnUpdate = { Enabled = true } }; // Open update form with scode-only overload
                        f.ShowDialog(); // Show modal dialog
                    } catch (Exception ex)
                    {
                        MessageBox.Show("Edit failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Error feedback
                    }
                    LoadSalary(); // Refresh grid after potential update
                }

            }
        }
        private void ptbAdd_Click(object sender, EventArgs e) { // Click add button/picture
            var f = new UpdateSalaryForm(); // Create empty update form
            f.btnAdd.Enabled = true; // Enable Add button
            f.btnUpdate.Enabled = false; // Disable Update button
            f.ShowDialog(); // Show modal
            LoadSalary(); // Refresh after adding
        }
        private void txtSearch_TextChanged(object sender, EventArgs e) { // Live search textbox changed
            if (txtSearch.Text == "") // If cleared
            {
                LoadSalary(); // Reload all
                return; // Exit handler
            }
            string term = txtSearch.Text.Trim(); // Search term
            try
            {
                ResetContext(); // Fresh context for query
                dgvSalary.AutoGenerateColumns = false; // Use predefined columns

                var rows = db.Salaries // Query salaries with filter
                    .Where(m => (m.EmployeeName ?? string.Empty).Contains(term) || m.EmployeeID.ToString().Contains(term)) // Match by name or ID
                    .Select(p => new { // Project required fields
                        p.EmployeeID,
                        p.EmployeeName,
                        p.Scode,
                        p.Salary1,
                        p.Period,
                        p.From,
                        p.To,
                        p.Paydate,
                        p.UnpaidDays
                    })
                    .ToList(); // Execute

                dgvSalary.DataSource = rows // Bind transformed results
                    .Select(p => new
                    {
                        EmpID = p.EmployeeID,
                        EmpName = p.EmployeeName,
                        Scode = p.Scode,
                        Salary1 = CurrencyFormatter.Format(p.Salary1),
                        Period = p.Period,
                        From = p.From,
                        To = p.To,
                        Paydate = p.Paydate,
                        totalsal = CurrencyFormatter.Format(ComputeNetTotalLong(p.Salary1, p.From, p.To, p.UnpaidDays)),
                        UnpaidDays = p.UnpaidDays
                    })
                    .ToList(); // Materialize for DataSource

                long totalAll = rows.Sum(s => ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays)); // Sum filtered totals
                lbltotal.Text = CurrencyFormatter.Format(totalAll) + "$"; // Display filtered grand total
            } catch (Exception ex)
            {
                MessageBox.Show("Search failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Error feedback
            }
        }

        private void button1_Click(object sender, EventArgs e) { // Open report button handler
            ReportForm reportForm = new ReportForm(); // Create report form
            reportForm.ShowDialog(); // Show as modal dialog
        }

        private void dgvSalary_CellContentClick(object sender, DataGridViewCellEventArgs e) { // CellContentClick (unused)

        }
    }
}
