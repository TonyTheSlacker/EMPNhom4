using Microsoft.Reporting.WinForms;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Globalization; // added for Vietnamese currency formatting

namespace EmployeeManagementSystem {
    public partial class ReportForm : Form {
        readonly EmployeeDataContext db = new EmployeeDataContext();
        public ReportForm() {
            InitializeComponent();
        }

        // Helper to format number like 123.000.000 VNĐ (Vietnamese thousands separator '.')
        private string FormatVnd(long amount) {
            if (amount < 0) amount = 0; // no negatives in this context
            return amount.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ"; // N0 uses grouping and no decimals
        }
        private string FormatVnd(int amount) { return FormatVnd((long)amount); }

        private string ResolveReportDefinition(string fileName, out bool embedded) {
            embedded = false;
            // Try embedded resource first
            var asm = Assembly.GetExecutingAssembly();
            var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName != null)
            {
                embedded = true;
                return resourceName;
            }

            // Physical path search (output dir + parent levels + project folder)
            string[] roots = new[] {
                Application.StartupPath,
                Environment.CurrentDirectory
            };
            foreach (var root in roots)
            {
                var p = Path.Combine(root, fileName);
                if (File.Exists(p))
                    return p;
            }
            var dir = new DirectoryInfo(Application.StartupPath);
            for (int i = 0; i < 6 && dir != null; i++)
            {
                var candidate = Path.Combine(dir.FullName, fileName);
                if (File.Exists(candidate))
                    return candidate;
                // project folder name might be the solution/project namespace - ensure we search for EmployeeManagementSystem
                var projCandidate = Path.Combine(dir.FullName, "EmployeeManagementSystem", fileName);
                if (File.Exists(projCandidate))
                    return projCandidate;
                dir = dir.Parent;
            }
            return null;
        }

        private bool TryLoadReportDefinition(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                // Ensure report viewer in local mode and clear previous definition
                reportViewer1.Reset();
                reportViewer1.ProcessingMode = ProcessingMode.Local;
                // Prevent designer-embedded resource from auto-loading
                reportViewer1.LocalReport.ReportEmbeddedResource = string.Empty;

                bool embedded;
                var reportDef = ResolveReportDefinition("Report1.rdlc", out embedded);
                if (reportDef == null)
                {
                    errorMessage = "Cannot locate Report1.rdlc. Set its Build Action to 'Embedded Resource' OR Copy to Output Directory.";
                    return false;
                }

                if (embedded)
                {
                    using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(reportDef))
                    {
                        if (s == null)
                        {
                            errorMessage = "Embedded resource stream not found: " + reportDef;
                            return false;
                        }
                        reportViewer1.LocalReport.LoadReportDefinition(s);
                    }
                }
                else
                {
                    reportViewer1.LocalReport.ReportPath = reportDef;
                }

                // Do not bind data sources here; leave it to export or later actions
                reportViewer1.LocalReport.DataSources.Clear();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : string.Empty);
                return false;
            }
        }

        private void ReportForm_Load(object sender, EventArgs e) {
            cbbGender.Items.Clear();
            cbbGender.Items.AddRange(new object[] { "All", "Male", "Female" });
            cbbGender.SelectedIndex = 0;
            rdoAllDay.Checked = true;

            // Populate names and IDs
            var employees = db.Employees.Select(emp => new { emp.EmpID, emp.EmpName }).ToList();
            cbbFullName.Items.Clear();
            cbbEmpID.Items.Clear();
            cbbFullName.Items.Add("All");
            cbbEmpID.Items.Add("All");
            foreach (var emp in employees)
            {
                cbbFullName.Items.Add(emp.EmpName);
                cbbEmpID.Items.Add(emp.EmpID.ToString());
            }
            cbbFullName.SelectedIndex = 0;
            cbbEmpID.SelectedIndex = 0;

            // Try to load RDLC safely to avoid invalid-definition crash
            string err;
            if (TryLoadReportDefinition(out err))
            {
                // Load all data initially
                RefreshReportData();
            }
            else
            {
                // Show the definition error if it occurs
                MessageBox.Show("Failed to load report definition: " + err, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e) {
            try
            {
                // Re-bind data before exporting to ensure filters are applied
                RefreshReportData();

                // Ask user where to save PDF
                using (var sfd = new SaveFileDialog { Filter = "PDF File|*.pdf", FileName = "EmployeeReport_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf" })
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        string mimeType, encoding, extension;
                        string[] streamIds; Warning[] warnings;
                        byte[] pdfBytes = reportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                        File.WriteAllBytes(sfd.FileName, pdfBytes);
                        MessageBox.Show("PDF exported to:\n" + sfd.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Report error: " + ex.Message + (ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : ""), "Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshReportData()
        {
            try
            {
                string gender = cbbGender.Text;
                string nameSelected = cbbFullName.Text;
                string idSelected = cbbEmpID.Text;

                var baseQuery = db.Salaries.AsQueryable();
                if (gender != "All")
                    baseQuery = baseQuery.Where(m => m.Employee.EmpGen == gender);
                if (nameSelected != "All")
                    baseQuery = baseQuery.Where(m => m.EmployeeName == nameSelected);
                if (idSelected != "All")
                    baseQuery = baseQuery.Where(m => m.Employee.EmpID.ToString() == idSelected);
                if (rdoFromto.Checked)
                {
                    DateTime fromDate = dtpkFrom.Value.Date;
                    DateTime toDate = dtpkTo.Value.Date;
                    baseQuery = baseQuery.Where(m => m.From >= fromDate && m.To <= toDate);
                }

                var projected = baseQuery.OrderBy(m => m.Employee.EmpID).Select(m => new
                {
                    EmployeeID = m.Employee.EmpID,
                    m.Scode,
                    m.EmployeeName,
                    Salary = m.Salary1,
                    Period = m.Period,
                    m.From,
                    m.To,
                    m.Paydate,
                    totalsal = m.totalsal ?? 0,
                    UnpaidDays = m.UnpaidDays
                }).ToList();

                var ds = new ReportDataSource("Salary", projected);
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(ds);

                reportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh report data: " + ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnChart_Click(object sender, EventArgs e) {
            using (var chartForm = new StatisticsChartForm())
            {
                chartForm.ShowDialog(this);
            }
        }

        private void rdoAllDay_CheckedChanged(object sender, EventArgs e) {
            bool enableRange = !rdoAllDay.Checked;
            dtpkFrom.Enabled = enableRange;
            dtpkTo.Enabled = enableRange;
        }

        private void Filters_Changed(object sender, EventArgs e)
        {
            if (this.Visible) // Only refresh if form is loaded
            {
                RefreshReportData();
            }
        }

        private void label3_Click(object sender, EventArgs e) {
        }
        private void cbbFullName_SelectedIndexChanged(object sender, EventArgs e) {
            Filters_Changed(sender, e);
        }
        private void cbbEmpID_SelectedIndexChanged(object sender, EventArgs e) {
            Filters_Changed(sender, e);
        }
    }
}
