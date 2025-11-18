using System; // Provides base types like DateTime, EventArgs
using System.Drawing; // GDI+ drawing types (Bitmap, Graphics, Pens, etc.)
using System.IO; // File and stream IO used for export
using System.Linq; // LINQ extension methods for querying collections and tables
using System.Windows.Forms; // WinForms UI classes (Form, Controls, Events)

namespace EmployeeManagementSystem { // Application namespace
    public partial class StatisticsChartForm : Form { // Partial form class definition (designer part elsewhere)
        private readonly EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL data context; loads connection string from Properties.Settings.Default.EmployeeManagementSystemConnectionString (app.config)
        private decimal maxValue; // Tracks maximum value in current dataset for scaling chart
        private System.Collections.Generic.List<(string Label, decimal Value)> currentData; // In-memory list of label/value pairs for chart rendering
        public StatisticsChartForm() { // Constructor
            InitializeComponent(); // Initialize UI components (auto-generated designer method)
        }
        private void StatisticsChartForm_Load(object sender, EventArgs e) { // Form Load event handler
            this.WindowState = FormWindowState.Maximized; // Maximize window on load
            lblTitle.Text = "Statistics & Charts"; // Set initial form title label
            cboMode.SelectedIndex = 0; // Default mode selection (Department chart)
            LoadEmployees(); // Populate employee combo box
            UpdateControlVisibility(); // Adjust visibility of controls based on mode
            LoadAndBind(); // Load data and refresh chart
        }
        private void cboMode_SelectedIndexChanged(object sender, EventArgs e) { // Mode combo selection change handler
            UpdateControlVisibility(); // Adjust control visibility
            LoadAndBind(); // Reload data for new mode
        }
        private void cboEmployee_SelectedIndexChanged(object sender, EventArgs e) { // Employee combo selection changed
            if (cboMode.SelectedIndex == 2) // Only relevant in employee monthly earnings mode
                LoadAndBind(); // Reload filtered data
        }
        private void chkRange_CheckedChanged(object sender, EventArgs e) { // Range checkbox toggled
            UpdateControlVisibility(); // Show/hide date range pickers
            LoadAndBind(); // Reload using range filter if applicable
        }
        private void dtFrom_ValueChanged(object sender, EventArgs e) { // From date changed
            if (cboMode.SelectedIndex == 2) // Only matters for employee mode
                LoadAndBind(); // Reload data
        }
        private void dtTo_ValueChanged(object sender, EventArgs e) { // To date changed
            if (cboMode.SelectedIndex == 2) // Only matters for employee mode
                LoadAndBind(); // Reload data
        }
        private void canvas_Resize(object sender, EventArgs e) { // Chart panel resize event
            canvas.Invalidate(); // Force repaint to adapt to new size
        }
        private void canvas_Paint(object sender, PaintEventArgs e) { // Paint event for chart area
            DrawChart(e.Graphics); // Delegate drawing to method with Graphics object
        }

        // Helpers to compute salary totals locally (avoid reading DB column with mismatched type)
        private int CalcMonths(DateTime from, DateTime to) { // Computes number of months between dates for salary logic
            from = from.Date; to = to.Date; // Normalize to date only
            if (to < from) return 0; // Invalid range guard
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month); // Base month difference
            if (to.Day < from.Day) months--; // Adjust if end day precedes start day within last month
            if (months < 1) months = 1; // Minimum of 1 month if positive span
            return months; // Return computed month count
        }
        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int unpaidDays) { // Calculates net salary total
            var f = from.Date; var t = to.Date; // Normalize dates
            if (t < f || monthlySalary <= 0) return 0L; // Invalid input early return
            int months = CalcMonths(f, t); // Compute months span
            if (months <= 0) return 0L; // Guard again
            long gross = (long)months * (long)monthlySalary; // Gross salary = months * monthly salary
            int rangeDays = (t - f).Days + 1; // Inclusive day span
            int ud = Math.Min(Math.Max(unpaidDays, 0), Math.Max(rangeDays, 0)); // Clamp unpaid days 0..rangeDays
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m); // Approx daily rate (ceiling)
            decimal deduction = ud * dailyRate; // Deduction for unpaid days
            decimal net = ((decimal)gross) - deduction; // Net after deduction
            if (net < 0) net = 0m; // Floor at zero
            if (net > long.MaxValue) return long.MaxValue; // Prevent overflow casting to long
            return (long)net; // Return long total
        }

        private void LoadEmployees() { // Populate employee combo box from database
            try
            {
                var list = db.Employees.OrderBy(e => e.EmpName).Select(e => new { e.EmpID, e.EmpName }).ToList(); // Query employees (ID + Name)
                cboEmployee.Items.Clear(); // Clear existing items
                cboEmployee.Items.Add("(Select Employee)"); // Add placeholder first item
                foreach (var emp in list) // Iterate employee results
                    cboEmployee.Items.Add(emp.EmpID + " - " + emp.EmpName); // Format and add each employee display string
                cboEmployee.SelectedIndex = list.Count > 0 ? 1 : 0; // Pre-select first employee if any exist
            } catch { } // Silent catch (ignore load errors)
        }
        private int? SelectedEmployeeId() { // Parse selected employee ID from combo box text
            if (cboEmployee.SelectedIndex <= 0) // Placeholder or nothing selected
                return null; // Return null (no ID)
            var text = cboEmployee.SelectedItem.ToString(); // Get selected item text
            int dash = text.IndexOf('-'); // Find dash separator
            if (dash > 0)
            {
                int id; // Temp variable for parsed ID
                if (int.TryParse(text.Substring(0, dash).Trim(), out id)) // Attempt parse prefix to int
                    return id; // Return parsed ID
            }
            return null; // Parsing failed
        }
        private string SelectedEmployeeName() { // Extract selected employee name
            if (cboEmployee.SelectedIndex <= 0) // Placeholder selected
                return null; // No name
            var text = cboEmployee.SelectedItem.ToString(); // Selected item text
            int dash = text.IndexOf('-'); // Dash separator position
            if (dash > 0 && dash + 1 < text.Length) // Ensure characters after dash
                return text.Substring(dash + 1).Trim(); // Return trimmed name portion
            return null; // Fallback if parsing fails
        }
        private void UpdateControlVisibility() { // Toggle visibility of controls based on mode and range selection
            bool showEmp = (cboMode.SelectedIndex == 2); // Employee mode flag
            lblEmp.Visible = showEmp; // Show/hide employee label
            cboEmployee.Visible = showEmp; // Show/hide employee combo
            chkRange.Visible = showEmp; // Show/hide range checkbox
            bool showRange = showEmp && chkRange.Checked; // Range controls visible only if employee mode + checked
            lblFrom.Visible = showRange; // From label visibility
            dtFrom.Visible = showRange; // From date picker visibility
            lblTo.Visible = showRange; // To label visibility
            dtTo.Visible = showRange; // To date picker visibility
        }
        private void LoadAndBind() { // Load data according to current mode and bind chart
            try
            {
                if (cboMode.SelectedIndex == 0) // Mode: Salary by Department
                {
                    lblTitle.Text = "Salary by Department"; // Update title
                    // Read safe columns, compute totals in-memory to avoid invalid casts on totalsal
                    var rows = db.Salaries
                        .Select(s => new { Dept = s.Employee.Department.DepName, s.Salary1, s.From, s.To, s.UnpaidDays }) // Project needed fields
                        .ToList(); // Execute query

                    currentData = rows
                        .GroupBy(x => x.Dept) // Group salaries by department name
                        .Select(g => (
                            Label: g.Key, // Department label
                            Value: g.Sum(r => (decimal)ComputeNetTotalLong(r.Salary1, r.From, r.To, r.UnpaidDays)) // Sum computed net values
                        ))
                        .OrderByDescending(x => x.Value) // Sort descending by total
                        .ToList(); // Materialize to list
                } else if (cboMode.SelectedIndex == 1) // Mode: Total Salary by Month (global)
                {
                    lblTitle.Text = "Total Salary by Month"; // Update title
                    var rows = db.Salaries
                        .Select(s => new { s.Paydate, s.Salary1, s.From, s.To, s.UnpaidDays }) // Project fields needed for monthly grouping
                        .ToList(); // Execute query

                    var grouped = rows
                        .GroupBy(s => new { s.Paydate.Year, s.Paydate.Month }) // Group by year/month
                        .Select(g => new {
                            KeyDate = new DateTime(g.Key.Year, g.Key.Month, 1), // Represent month start
                            Total = g.Sum(r => (decimal)ComputeNetTotalLong(r.Salary1, r.From, r.To, r.UnpaidDays)) // Sum computed net totals
                        })
                        .OrderBy(x => x.KeyDate) // Order chronologically
                        .ToList(); // Materialize grouped list

                    currentData = grouped
                        .Select(x => (Label: x.KeyDate.ToString("MM/yyyy"), Value: x.Total)) // Format label + value tuple
                        .ToList(); // Create list for chart
                } else // Mode: Employee Earnings by Month
                {
                    lblTitle.Text = "Employee Earnings by Month"; // Update title
                    var query = db.Salaries.AsQueryable(); // Start base query
                    int? empId = SelectedEmployeeId(); // Get selected employee ID (if any)
                    if (empId.HasValue) // If specific employee selected
                        query = query.Where(s => s.Employee.EmpID == empId.Value); // Filter by employee
                    if (chkRange.Checked) // If range filter enabled
                    {
                        var f = dtFrom.Value.Date; // From date
                        var t = dtTo.Value.Date; // To date
                        query = query.Where(s => s.Paydate.Date >= f && s.Paydate.Date <= t); // Filter paydate within range
                    }

                    var rows = query
                        .Select(s => new { s.Paydate, s.Salary1, s.From, s.To, s.UnpaidDays }) // Project needed fields
                        .ToList(); // Execute query

                    var grouped = rows
                        .GroupBy(s => new { s.Paydate.Year, s.Paydate.Month }) // Group by paydate year/month
                        .Select(g => new {
                            KeyDate = new DateTime(g.Key.Year, g.Key.Month, 1), // Month start representation
                            Total = g.Sum(r => (decimal)ComputeNetTotalLong(r.Salary1, r.From, r.To, r.UnpaidDays)) // Sum computed net totals
                        })
                        .OrderBy(x => x.KeyDate) // Chronological order
                        .ToList(); // Materialize

                    currentData = grouped
                        .Select(x => (Label: x.KeyDate.ToString("MM/yyyy"), Value: x.Total)) // Map to label/value tuples
                        .ToList(); // Produce final list
                }
                maxValue = currentData.Count > 0 ? currentData.Max(d => d.Value) : 1m; // Determine max value for scaling (avoid zero)
                canvas.Invalidate(); // Request chart repaint
            } catch (Exception ex) { MessageBox.Show("Failed to load chart data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } // Error feedback
        }
        private void DrawChart(Graphics g) { // Master chart drawing routine
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Improve rendering quality
            var rect = canvas.ClientRectangle; // Full client rectangle of canvas
            rect.Inflate(-40, -50); // Apply margins
            if (rect.Width <= 0 || rect.Height <= 0) // Guard against invalid drawing region
                return; // Exit if not drawable
            using (var bg = new SolidBrush(Color.White)) // Background brush
                g.FillRectangle(bg, canvas.ClientRectangle); // Fill background
            using (var axis = new Pen(Color.Gray, 1)) // Pen for axes
            {
                g.DrawLine(axis, rect.Left, rect.Bottom, rect.Right, rect.Bottom); // X-axis
                g.DrawLine(axis, rect.Left, rect.Bottom, rect.Left, rect.Top); // Y-axis
            }
            if (currentData == null || currentData.Count == 0) // No data safeguard
            {
                using (var f = new Font("Century Gothic", 11, FontStyle.Italic)) // Font for message
                using (var b = new SolidBrush(Color.Gray)) // Brush for text
                {
                    var s = g.MeasureString("No data", f); // Measure text size
                    g.DrawString("No data", f, b, rect.Left + (rect.Width - s.Width) / 2, rect.Top + (rect.Height - s.Height) / 2); // Center draw message
                }
                return; // Exit after drawing message
            }
            if (cboMode.SelectedIndex == 0) // Department mode -> bar chart
                DrawBars(g, rect); // Draw bars
            else // Other modes -> line chart
                DrawLine(g, rect); // Draw line chart
        }
        private void DrawBars(Graphics g, Rectangle rect) { // Draw bar chart for department salary totals
            int n = currentData.Count; // Number of bars
            int spacing = 10; // Space between bars
            int barWidth = Math.Max(14, (rect.Width - spacing * (n + 1)) / Math.Max(1, n)); // Compute bar width with min limit
            using (var br = new SolidBrush(Color.SteelBlue)) // Brush for bars
            using (var txt = new SolidBrush(Color.Black)) // Brush for text
            using (var small = new Font("Century Gothic", 8f)) // Small font for labels
            using (var grid = new Pen(Color.Gainsboro, 1)) // Pen for grid lines
            {
                for (int i = 1; i <= 5; i++) // Draw horizontal grid + ticks (5 segments)
                {
                    float y = rect.Bottom - i * (rect.Height / 5f); // Y coordinate for grid line
                    g.DrawLine(grid, rect.Left, y, rect.Right, y); // Draw grid line
                    string tick = string.Format("{0:N0}", (maxValue / 5m) * i); // Tick label value
                    var sz = g.MeasureString(tick, small); // Measure tick text
                    g.DrawString(tick, small, txt, rect.Left - sz.Width - 8, y - sz.Height / 2f); // Draw tick left of axis
                }
                int x = rect.Left + spacing; // Starting X for first bar
                for (int i = 0; i < n; i++) // Iterate data points
                {
                    var d = currentData[i]; // Current data tuple
                    int h = (int)Math.Round((double)(d.Value / maxValue * rect.Height)); // Scaled bar height
                    var bar = new Rectangle(x, rect.Bottom - h, barWidth, h); // Bar rectangle
                    g.FillRectangle(br, bar); // Fill bar
                    string val = string.Format("{0:N0}", d.Value); // Value label text
                    var vs = g.MeasureString(val, small); // Measure value text
                    g.DrawString(val, small, txt, bar.Left + (bar.Width - vs.Width) / 2f, bar.Top - vs.Height - 2); // Draw value above bar
                    var xs = g.MeasureString(d.Label, small); // Measure label text
                    if (xs.Width > barWidth + 12) // If label too wide draw rotated
                    {
                        var st = g.Save(); // Save graphics state
                        g.TranslateTransform(bar.Left + bar.Width / 2f, rect.Bottom + 6); // Move origin to label position
                        g.RotateTransform(-45); // Rotate for better fit
                        g.DrawString(d.Label, small, txt, -xs.Width / 2f, 0); // Draw rotated label
                        g.Restore(st); // Restore state
                    } else
                    {
                        g.DrawString(d.Label, small, txt, bar.Left + (bar.Width - xs.Width) / 2f, rect.Bottom + 6); // Draw normal label centered
                    }
                    x += barWidth + spacing; // Advance X for next bar
                }
            }
        }
        private void DrawLine(Graphics g, Rectangle rect) { // Draw line chart for monthly totals
            int n = currentData.Count; // Number of points
            var pts = new System.Collections.Generic.List<PointF>(n); // List to hold plotted points
            for (int i = 0; i < n; i++) // Build point list
            {
                float t = n == 1 ? 0.5f : (float)i / (n - 1); // Relative position (center if single point)
                float x = rect.Left + t * rect.Width; // X coordinate
                float y = rect.Bottom - (float)((double)(currentData[i].Value / maxValue) * rect.Height); // Y coordinate (invert for chart)
                pts.Add(new PointF(x, y)); // Add point
            }
            using (var grid = new Pen(Color.Gainsboro, 1)) // Grid line pen
            using (var line = new Pen(Color.SteelBlue, 2.5f)) // Line pen
            using (var marker = new SolidBrush(Color.SteelBlue)) // Marker brush
            using (var txt = new SolidBrush(Color.Black)) // Text brush
            using (var small = new Font("Century Gothic", 8f)) // Label font
            {
                for (int i = 1; i <= 5; i++) // Draw horizontal grid & ticks
                {
                    float y = rect.Bottom - i * (rect.Height / 5f); // Y coordinate for grid line
                    g.DrawLine(grid, rect.Left, y, rect.Right, y); // Draw grid line
                    string tick = string.Format("{0:N0}", (maxValue / 5m) * i); // Tick label value
                    var sz = g.MeasureString(tick, small); // Measure tick text
                    g.DrawString(tick, small, txt, rect.Left - sz.Width - 8, y - sz.Height / 2f); // Draw tick label
                }
                if (pts.Count >= 2) // Draw connecting lines if more than 1 point
                    g.DrawLines(line, pts.ToArray()); // Draw polyline
                for (int i = 0; i < n; i++) // Draw markers and labels
                {
                    var p = pts[i]; // Current point
                    g.FillEllipse(marker, p.X - 3, p.Y - 3, 6, 6); // Draw circular marker
                    string v = string.Format("{0:N0}", currentData[i].Value); // Value string
                    var vs = g.MeasureString(v, small); // Measure value text
                    g.DrawString(v, small, txt, p.X - vs.Width / 2f, p.Y - vs.Height - 4); // Draw value above point
                    var xs = g.MeasureString(currentData[i].Label, small); // Measure label text
                    g.DrawString(currentData[i].Label, small, txt, p.X - xs.Width / 2f, rect.Bottom + 6); // Draw X-axis label
                }
            }
        }
        private Bitmap CreateChartBitmap() { // Create bitmap snapshot of chart for export/print
            int w = Math.Max(64, canvas.Width); // Ensure minimum width
            int h = Math.Max(64, canvas.Height); // Ensure minimum height
            var bmp = new Bitmap(w, h); // Create bitmap
            canvas.DrawToBitmap(bmp, new Rectangle(0, 0, w, h)); // Render canvas contents to bitmap
            return bmp; // Return bitmap
        }
        private void btnExportImage_Click(object sender, EventArgs e) { // Export chart image button handler
            try
            {
                using (var bmp = CreateChartBitmap()) // Create bitmap snapshot
                using (var sfd = new SaveFileDialog { Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp", FileName = BuildBaseFileName() }) // Configure save dialog
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK) // Show dialog and confirm
                    {
                        var ext = Path.GetExtension(sfd.FileName); // Extract selected extension
                        System.Drawing.Imaging.ImageFormat fmt = System.Drawing.Imaging.ImageFormat.Png; // Default format
                        if (!string.IsNullOrEmpty(ext)) // If user provided extension
                        {
                            switch (ext.ToLowerInvariant()) // Map extension to format
                            {
                                case ".jpg":
                                case ".jpeg":
                                    fmt = System.Drawing.Imaging.ImageFormat.Jpeg; // JPEG format
                                    break;
                                case ".bmp":
                                    fmt = System.Drawing.Imaging.ImageFormat.Bmp; // BMP format
                                    break;
                                default:
                                    fmt = System.Drawing.Imaging.ImageFormat.Png; // Fallback to PNG
                                    break;
                            }
                        } else
                        {
                            sfd.FileName = sfd.FileName + ".png"; // Append default PNG extension
                        }
                        bmp.Save(sfd.FileName, fmt); // Save bitmap to chosen file
                        MessageBox.Show("Chart image saved to:\n" + sfd.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information); // Success message
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Failed to export image: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error); } // Error message
        }

        private string BuildBaseFileName() { // Build base filename (without extension) incorporating state
            // Base title from form label
            string baseTitle = string.IsNullOrWhiteSpace(lblTitle.Text) ? "Chart" : lblTitle.Text.Replace(' ', '_'); // Replace spaces with underscores

            // If employee mode, include ID + Name
            if (cboMode.SelectedIndex == 2) // Employee earnings mode
            {
                var id = SelectedEmployeeId(); // Selected employee ID
                var name = SelectedEmployeeName(); // Selected employee name
                if (id.HasValue && !string.IsNullOrEmpty(name)) // Validate both
                    baseTitle = "Employee_" + id.Value + "_" + name.Replace(' ', '_'); // Build employee-specific title
            }

            // Optional range part when employee mode + range selected
            string rangePart = ""; // Initialize range part
            if (cboMode.SelectedIndex == 2 && chkRange.Checked) // Condition for range suffix
            {
                rangePart = "_Range_" + dtFrom.Value.ToString("yyyyMMdd") + "-" + dtTo.Value.ToString("yyyyMMdd"); // Append range formatted dates
            }

            // Printable date/time stamp (current print time)
            string printedStamp = DateTime.Now.ToString("yyyyMMdd_HHmm"); // Timestamp for uniqueness

            return baseTitle + rangePart + "_Printed_" + printedStamp; // Final combined filename
        }

        private void btnPrint_Click(object sender, EventArgs e) { // Export chart to PDF button handler
            try
            {
                using (var bmp = CreateChartBitmap()) // Create chart bitmap
                using (var sfd = new SaveFileDialog { Filter = "PDF File|*.pdf", FileName = BuildBaseFileName() + ".pdf" }) // Configure save dialog for PDF
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK) // Confirm save
                    {
                        byte[] jpgBytes; // Buffer for JPEG-compressed chart image
                        using (var ms = new MemoryStream()) // Memory stream for JPEG encode
                        {
                            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // Encode bitmap as JPEG
                            jpgBytes = ms.ToArray(); // Extract byte array
                        }
                        int imgWpt = (int)(bmp.Width * 72.0 / bmp.HorizontalResolution); // Convert image width to PDF points
                        int imgHpt = (int)(bmp.Height * 72.0 / bmp.VerticalResolution); // Convert image height to PDF points
                        int pageW = Math.Max(imgWpt + 40, 595); // Ensure at least A4 width (595pt) with padding
                        int pageH = Math.Max(imgHpt + 80, 842); // Ensure at least A4 height (842pt) with padding
                        int imgX = (pageW - imgWpt) / 2; // Center image horizontally
                        int imgY = (pageH - imgHpt) / 2; // Center image vertically
                        using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write)) // Create PDF file
                        using (var bw = new BinaryWriter(fs)) // Binary writer for PDF syntax
                        {
                            bw.Write(System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n")); // PDF header
                            var offsets = new System.Collections.Generic.List<long>(); // Track object offsets for xref
                            Action<string> writeObj = (s) => { offsets.Add(fs.Position); bw.Write(System.Text.Encoding.ASCII.GetBytes(s)); }; // Helper to write object and record offset
                            writeObj("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n"); // Catalog object
                            writeObj("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n"); // Pages root object
                            // Correct page dictionary (removed duplicate >>)
                            writeObj(string.Format("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0} {1}] /Resources << /XObject << /Im0 4 0 R >> /ProcSet [/PDF /ImageC] >> /Contents 5 0 R >>endobj\n", pageW, pageH)); // Single page object
                            writeObj(string.Format("4 0 obj<< /Type /XObject /Subtype /Image /Width {0} /Height {1} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {2} >>stream\n", bmp.Width, bmp.Height, jpgBytes.Length)); // Image object header
                            bw.Write(jpgBytes); // Write JPEG bytes
                            bw.Write(System.Text.Encoding.ASCII.GetBytes("\nendstream\nendobj\n")); // End image object
                            string content = string.Format("q\n{0} 0 0 {1} {2} {3} cm /Im0 Do\nQ\n", imgWpt, imgHpt, imgX, imgY); // Content stream (draw image with transform)
                            writeObj(string.Format("5 0 obj<< /Length {0} >>stream\n{1}endstream\nendobj\n", content.Length, content)); // Content object
                            long xrefPos = fs.Position; // Record xref start position
                            bw.Write(System.Text.Encoding.ASCII.GetBytes("xref\n0 6\n0000000000 65535 f \n")); // XRef header
                            for (int i = 0; i < offsets.Count; i++) // Write object offsets
                                bw.Write(System.Text.Encoding.ASCII.GetBytes(offsets[i].ToString("D10") + " 00000 n \n")); // Each object entry
                            bw.Write(System.Text.Encoding.ASCII.GetBytes("trailer<< /Size 6 /Root 1 0 R >>\nstartxref\n" + xrefPos + "\n%%EOF\n")); // Trailer and EOF
                        }
                        MessageBox.Show("PDF exported to:\n" + sfd.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information); // Success message
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Failed to export PDF: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error); } // Error message
        }
    }
}
