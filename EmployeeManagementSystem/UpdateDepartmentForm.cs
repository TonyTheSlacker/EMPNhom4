using System; // Base types
using System.Collections.Generic; // Collections (not used directly here)
using System.ComponentModel; // Component model (designer support)
using System.Data; // Data types
using System.Drawing; // Drawing types
using System.Linq; // LINQ operators
using System.Runtime.InteropServices; // For Win32 interop (dragging window)
using System.Text; // Text utilities
using System.Threading.Tasks; // Tasks (not used directly here)
using System.Windows.Forms; // WinForms UI

namespace EmployeeManagementSystem
{
    public partial class UpdateDepartmentForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1; // Win32 message: non-client left button down
        public const int HT_CAPTION = 0x2; // Hit-test caption (used to drag the window)

        [DllImportAttribute("user32.dll")] // Import SendMessage for dragging the form
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")] // Import ReleaseCapture before moving
        public static extern bool ReleaseCapture();

        Department department = null; // Currently edited department (null when adding)
        EmployeeDataContext db = new EmployeeDataContext(); // LINQ-to-SQL context: uses Properties.Settings.Default.EmployeeManagementSystemConnectionString from app.config
        public UpdateDepartmentForm(Department dep=null) // Constructor accepts optional department to edit
        {
            department = dep; // Store input entity reference
            InitializeComponent(); // Initialize UI controls
        }

        private void ptbClose_Click(object sender, EventArgs e) // Close picture/button click handler
        {
            this.Dispose(); // Close the form
        }

        private void btnAdd_Click(object sender, EventArgs e) // Add new department button
        {
            if(string.IsNullOrEmpty(txtName.Text)) // Validate required name field
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Warn user
            }
            else
            {
                Department dep = new Department();  // Create new Department entity
                dep.DepName=txtName.Text.Trim(); // Set department name
                db.Departments.InsertOnSubmit(dep); // Queue insert into Departments table via DataContext
                db.SubmitChanges(); // Persist changes to database
                MessageBox.Show("Inserted Sucessfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform success
                this.Dispose(); // Close form
            } 
                
        }

        private void UpdateDepartmentForm_Load(object sender, EventArgs e) // Form Load handler
        {
            if(department!=null) // If editing an existing department
            {
                txtName.Text = department.DepName; // Prefill department name from entity
            }    
        }

        private void btnUpdate_Click(object sender, EventArgs e) // Update department button
        {
            if(string.IsNullOrEmpty(txtName.Text)) // Validate name
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Warn
                return; // Abort
            }
            else
            {
                Department dep = db.Departments.SingleOrDefault(d => d.DepId == department.DepId); // Load current department from DB by ID
                dep.DepName = txtName.Text; // Update name
                db.SubmitChanges(); // Save to DB
                MessageBox.Show("Updated Sucessfully", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform success
                this.Dispose(); // Close form
            } 
                
        }

        private void pnltop_MouseDown(object sender, MouseEventArgs e) // Allow dragging the form by top panel
        {
            if (e.Button == MouseButtons.Left) // On left mouse button
            {
                ReleaseCapture(); // Release capture
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); // Send move command
            }
        }
    }
}
