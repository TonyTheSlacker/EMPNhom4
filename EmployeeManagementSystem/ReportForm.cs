using Microsoft.Reporting.WinForms; // Reference to ReportViewer control types
using System; // Base system types (DateTime, Exception, etc.)
using System.Globalization; // added for Vietnamese currency formatting vnđ
using System.IO; // For file operations (PDF export)
using System.Linq; // LINQ queries over data context
using System.Reflection; // For loading embedded RDLC resources
using System.Windows.Forms; // WinForms UI framework

namespace EmployeeManagementSystem { // Application namespace
    public partial class ReportForm : Form { // Report form partial class (designer part elsewhere)
        readonly EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context using connection string in app.config
        private bool _syncingUi; // Flag to prevent recursive combo box sync events
        public ReportForm() { // Constructor
            InitializeComponent(); // Initialize UI controls from designer
        }

        // Helper to format number like 123.000.000 VNĐ (Vietnamese thousands separator '.')
        private string FormatVnd(long amount) { // Overload for long amounts
            if (amount < 0)
                amount = 0; // Guard against negative values
            return amount.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ"; // Format with thousand groups and append currency
        }
        private string FormatVnd(int amount) {
            return FormatVnd((long)amount);
        } // Convenience overload for int

        // Salary total helpers (avoid reading mismatched DB column types)
        private int CalcMonths(DateTime from, DateTime to) { // Calculate number of salary months between dates
            from = from.Date;
            to = to.Date; // Strip time component
            if (to < from)
                return 0; // Invalid range -> 0 months
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month); // Raw month difference
            if (to.Day < from.Day)
                months--; // Adjust if end day earlier than start day
            if (months < 1)
                months = 1; // Minimum 1 month for valid span
            return months; // Return computed months
        }
        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int unpaidDays) { // Compute net salary over a period
            var f = from.Date;
            var t = to.Date; // Normalize dates
            if (t < f || monthlySalary <= 0)
                return 0L; // Invalid input guard
            int months = CalcMonths(f, t); // Months span
            if (months <= 0)
                return 0L; // Guard again
            long gross = (long)months * (long)monthlySalary; // Gross salary total
            int rangeDays = (t - f).Days + 1; // Inclusive day count
            int ud = Math.Min(Math.Max(unpaidDays, 0), Math.Max(rangeDays, 0)); // Clamp unpaid days to valid range
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Approximate daily rate (ceil)
            decimal deduction = ud * dailyRate; // Deduction for unpaid days
            decimal net = ((decimal)gross) - deduction; // Net after deduction
            if (net < 0)
                net = 0m; // Prevent negative
            if (net > long.MaxValue)
                return long.MaxValue; // Overflow protection
            return (long)net; // Cast to long and return
        }

        private string ResolveReportDefinition(string fileName, out bool embedded) { // Locate RDLC file or embedded resource
            embedded = false; // Initialize embedded flag
            // Try embedded resource first
            var asm = Assembly.GetExecutingAssembly(); // Get current assembly
            var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)); // Find matching resource name
            if (resourceName != null) // If found
            {
                embedded = true; // Mark as embedded
                return resourceName; // Return resource name
            }

            // Physical path search (output dir + parent levels + project folder)
            string[] roots = new[] { // Candidate starting directories
                Application.StartupPath, // Application startup path
                Environment.CurrentDirectory // Current working directory
            };
            foreach (var root in roots) // Enumerate roots
            {
                var p = Path.Combine(root, fileName); // Build path
                if (File.Exists(p)) // If file exists
                    return p; // Return path
            }
            var dir = new DirectoryInfo(Application.StartupPath); // Start directory info at startup path
            for (int i = 0; i < 6 && dir != null; i++) // Traverse up to 6 parent levels
            {
                var candidate = Path.Combine(dir.FullName, fileName); // Direct path candidate
                if (File.Exists(candidate)) // Exists check
                    return candidate; // Return if found
                // project folder name might be the solution/project namespace - ensure we search for EmployeeManagementSystem
                var projCandidate = Path.Combine(dir.FullName, "EmployeeManagementSystem", fileName); // Candidate inside project folder
                if (File.Exists(projCandidate)) // Exists check
                    return projCandidate; // Return if found
                dir = dir.Parent; // Move up one level
            }
            return null; // Not found
        }

        private bool TryLoadReportDefinition(out string errorMessage) // Load RDLC definition into viewer
        {
            errorMessage = null; // Initialize error message
            try // Begin try block
            {
                // Ensure report viewer in local mode and clear previous definition
                reportViewer1.Reset(); // Reset ReportViewer state
                reportViewer1.ProcessingMode = ProcessingMode.Local; // Set processing mode to local
                // Prevent designer-embedded resource from auto-loading
                reportViewer1.LocalReport.ReportEmbeddedResource = string.Empty; // Clear embedded resource name

                bool embedded; // Will hold embedded flag
                var reportDef = ResolveReportDefinition("Report1.rdlc", out embedded); // Attempt to resolve RDLC path/resource
                if (reportDef == null) // If not found
                {
                    errorMessage = "Cannot locate Report1.rdlc. Set its Build Action to 'Embedded Resource' OR Copy to Output Directory."; // Set error message
                    return false; // Return failure
                }

                if (embedded) // If embedded resource
                {
                    using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(reportDef)) // Open resource stream
                    {
                        if (s == null) // If stream null
                        {
                            errorMessage = "Embedded resource stream not found: " + reportDef; // Set error
                            return false; // Fail
                        }
                        reportViewer1.LocalReport.LoadReportDefinition(s); // Load RDLC from stream
                    }
                } else // Physical file path
                {
                    reportViewer1.LocalReport.ReportPath = reportDef; // Assign RDLC file path
                }

                // Do not bind data sources here; leave it to export or later actions
                reportViewer1.LocalReport.DataSources.Clear(); // Clear any existing data sources
                return true; // Success
            } catch (Exception ex) // Catch any exceptions
            {
                errorMessage = ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : string.Empty); // Compose error message
                return false; // Indicate failure
            }
        }

        private void ReportForm_Load(object sender, EventArgs e) { // Form Load event handler
            cbbGender.Items.Clear(); // Clear gender combo items
            cbbGender.Items.AddRange(new object[] { "All", "Male", "Female" }); // Populate gender filter options
            cbbGender.SelectedIndex = 0; // Select 'All'
            rdoAllDay.Checked = true; // Default radio: all days (no date range)

            // Populate names and IDs from employees that actually have salary rows to avoid empty selections
            var employees = db.Salaries.Select(s => new { s.Employee.EmpID, s.Employee.EmpName }) // Query salary table -> employee info
                                       .Distinct() // Remove duplicates
                                       .OrderBy(x => x.EmpName) // Order by name
                                       .ToList(); // Execute query
            cbbFullName.Items.Clear(); // Clear name combo
            cbbEmpID.Items.Clear(); // Clear ID combo
            cbbFullName.Items.Add("All"); // Add 'All' option for names
            cbbEmpID.Items.Add("All"); // Add 'All' option for IDs
            foreach (var emp in employees) // Iterate employee results
            {
                cbbFullName.Items.Add(emp.EmpName); // Add employee name item
                cbbEmpID.Items.Add(emp.EmpID.ToString()); // Add employee ID item
            }
            cbbFullName.SelectedIndex = 0; // Select 'All' names
            cbbEmpID.SelectedIndex = 0; // Select 'All' IDs

            // Try to load RDLC safely to avoid invalid-definition crash
            string err; // Variable for error message
            if (TryLoadReportDefinition(out err)) // Attempt load of RDLC
            {
                // Load all data initially
                RefreshReportData(); // Fill report viewer with data
            } else // Failed to load RDLC
            {
                // Show the definition error if it occurs
                MessageBox.Show("Failed to load report definition: " + err, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Display message
            }
        }
        private void btnExport_Click(object sender, EventArgs e) { // Export button click handler
            try // Begin try block
            {
                // Re-bind data before exporting to ensure filters are applied
                RefreshReportData(); // Refresh data according to current filters

                // Ask user where to save PDF
                using (var sfd = new SaveFileDialog { Filter = "PDF File|*.pdf", FileName = "EmployeeReport_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf" }) // Configure save dialog
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK) // If user confirms
                    {
                        string mimeType, encoding, extension; // Out parameters for render
                        string[] streamIds;
                        Warning[] warnings; // Additional render output arrays
                        byte[] pdfBytes = reportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings); // Render report to PDF byte array
                        File.WriteAllBytes(sfd.FileName, pdfBytes); // Write PDF file
                        MessageBox.Show("PDF exported to:\n" + sfd.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform user of success
                    }
                }
            } catch (Exception ex) // Catch export errors
            {
                MessageBox.Show("Report error: " + ex.Message + (ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : ""), "Report", MessageBoxButtons.OK, MessageBoxIcon.Error); // Display error message
            }
        }

        private void RefreshReportData() // Build dataset and bind to ReportViewer
        {
            try // Begin try block
            {
                string gender = cbbGender.Text.Trim(); // Read gender filter
                string nameSelected = cbbFullName.Text.Trim(); // Read name filter
                string idSelected = cbbEmpID.Text.Trim(); // Read ID filter

                var baseQuery = db.Salaries.AsQueryable(); // Start base LINQ query over salaries

                // Normalize gender filter (case-insensitive, trims)
                if (!string.Equals(gender, "All", StringComparison.OrdinalIgnoreCase)) // If gender is filtered
                    baseQuery = baseQuery.Where(m => m.Employee.EmpGen != null && m.Employee.EmpGen.ToLower() == gender.ToLower()); // Apply gender filter

                // Prefer filtering by EmployeeID; if name chosen, resolve ID and filter by ID
                int empIdVal; // Temporary parsed employee ID
                int? empIdFilter = null; // Nullable ID filter
                if (!string.Equals(idSelected, "All", StringComparison.OrdinalIgnoreCase) && int.TryParse(idSelected, out empIdVal)) // If ID selected and parsed
                {
                    empIdFilter = empIdVal; // Set ID filter
                } else if (!string.Equals(nameSelected, "All", StringComparison.OrdinalIgnoreCase)) // Else if name selected
                {
                    empIdFilter = db.Employees.Where(e => e.EmpName == nameSelected).Select(e => (int?)e.EmpID).FirstOrDefault(); // Lookup ID by name
                }
                if (empIdFilter.HasValue) // If we have an ID filter
                    baseQuery = baseQuery.Where(m => m.Employee.EmpID == empIdFilter.Value); // Apply employee filter

                if (rdoFromto.Checked) // If custom date range selected
                {
                    DateTime fromDate = dtpkFrom.Value.Date; // Read from date
                    DateTime toDate = dtpkTo.Value.Date; // Read to date
                    baseQuery = baseQuery.Where(m => m.From >= fromDate && m.To <= toDate); // Apply date range filter
                }

                // Select only safe columns (DON'T read m.totalsal, which may have a mismatched DB type)
                var safeRows = baseQuery // Continue building query
                    .OrderBy(m => m.Employee.EmpID) // Order by employee ID
                    .Select(m => new
                    { // Project to anonymous type with required fields
                        EmployeeID = m.Employee.EmpID, // Employee ID
                        m.Scode, // Salary code
                        EmployeeName = m.Employee.EmpName, // Employee name
                        Salary1 = m.Salary1, // Monthly salary
                        Period = m.Period, // Period months stored
                        m.From, // From date
                        m.To, // To date
                        m.Paydate, // Pay date
                        UnpaidDays = m.UnpaidDays // Unpaid days
                    })
                    .ToList(); // Execute query and load into memory

                var projected = safeRows.Select(m => new
                { // Project again adding computed total salary
                    EmployeeID = m.EmployeeID, // ID
                    m.Scode, // Code
                    m.EmployeeName, // Name
                    Salary = m.Salary1, // Monthly salary
                    Period = m.Period, // Period
                    m.From, // From date
                    m.To, // To date
                    m.Paydate, // Pay date
                    totalsal = ComputeNetTotalLong(m.Salary1, m.From, m.To, m.UnpaidDays), // Computed total salary
                    UnpaidDays = m.UnpaidDays // Unpaid days
                }).ToList(); // Materialize projected list

                var ds = new ReportDataSource("Salary", projected); // Create ReportDataSource with name matching RDLC dataset
                reportViewer1.LocalReport.DataSources.Clear(); // Clear existing data sources
                reportViewer1.LocalReport.DataSources.Add(ds); // Add new data source

                reportViewer1.RefreshReport(); // Refresh report rendering
            } catch (Exception ex) // Catch data binding errors
            {
                MessageBox.Show("Failed to refresh report data: " + ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Show warning
            }
        }

        private void btnChart_Click(object sender, EventArgs e) { // Button to open chart form
            using (var chartForm = new StatisticsChartForm()) // Instantiate chart form inside using
            {
                chartForm.ShowDialog(this); // Show as modal dialog with this form as owner
            }
        }

        private void rdoAllDay_CheckedChanged(object sender, EventArgs e) { // Radio button change handler for all day
            bool enableRange = !rdoAllDay.Checked; // Range enabled only if not all-day selected
            dtpkFrom.Enabled = enableRange; // Enable/disable from picker
            dtpkTo.Enabled = enableRange; // Enable/disable to picker
        }

        private void Filters_Changed(object sender, EventArgs e) // Common handler for filter changes
        {
            if (this.Visible) // Ensure form loaded / visible
            {
                RefreshReportData(); // Refresh data according to updated filters
            }
        }

        private void label3_Click(object sender, EventArgs e) { // Empty label click handler (unused)
        }
        private void cbbFullName_SelectedIndexChanged(object sender, EventArgs e) { // FullName combo change handler
            if (_syncingUi)
            {
                Filters_Changed(sender, e);
                return;
            } // If syncing internally, just trigger refresh
            try // Begin try block
            {
                _syncingUi = true; // Set syncing flag
                if (string.Equals(cbbFullName.Text, "All", StringComparison.OrdinalIgnoreCase)) // If 'All' selected
                {
                    cbbEmpID.SelectedIndex = 0; // Match ID combo to 'All'
                } else // Specific name chosen
                {
                    var id = db.Employees.Where(x => x.EmpName == cbbFullName.Text) // Query employee by name
                                          .Select(x => (int?)x.EmpID) // Select ID (nullable)
                                          .FirstOrDefault(); // Get first or null
                    if (id.HasValue) // If ID found
                    {
                        string idText = id.Value.ToString(); // Convert to string
                        for (int i = 0; i < cbbEmpID.Items.Count; i++) // Iterate ID combo items
                        {
                            if (string.Equals(cbbEmpID.Items[i].ToString(), idText, StringComparison.Ordinal)) // Compare to found ID
                            {
                                cbbEmpID.SelectedIndex = i; // Select matching ID item
                                break; // Stop loop
                            }
                        }
                    }
                }
            } finally { _syncingUi = false; } // Always clear syncing flag
            Filters_Changed(sender, e); // Trigger data refresh
        }
        private void cbbEmpID_SelectedIndexChanged(object sender, EventArgs e) { // Employee ID combo change handler
            if (_syncingUi)
            {
                Filters_Changed(sender, e);
                return;
            } // Skip if internal sync in progress
            try // Begin try block
            {
                _syncingUi = true; // Set syncing flag
                if (string.Equals(cbbEmpID.Text, "All", StringComparison.OrdinalIgnoreCase)) // If 'All' selected
                {
                    cbbFullName.SelectedIndex = 0; // Sync name combo to 'All'
                } else // Specific ID chosen
                {
                    int id; // Temp parsed ID
                    if (int.TryParse(cbbEmpID.Text, out id)) // Parse ID text
                    {
                        var name = db.Employees.Where(x => x.EmpID == id) // Query employee by ID
                                                .Select(x => x.EmpName) // Select name
                                                .FirstOrDefault(); // Get first or null
                        if (!string.IsNullOrEmpty(name)) // If name found
                        {
                            for (int i = 0; i < cbbFullName.Items.Count; i++) // Iterate name items
                            {
                                if (string.Equals(cbbFullName.Items[i].ToString(), name, StringComparison.Ordinal)) // Match actual name
                                {
                                    cbbFullName.SelectedIndex = i; // Select matching name
                                    break; // Stop loop
                                }
                            }
                        }
                    }
                }
            } finally { _syncingUi = false; } // Clear syncing flag
            Filters_Changed(sender, e); // Trigger refresh
        }
    }
}
