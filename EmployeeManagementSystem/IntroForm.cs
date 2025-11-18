using System.Drawing; // Provides drawing types like Color, Font
using System.Linq; // LINQ extensions for querying controls
using System.Windows.Forms; // WinForms UI base classes

namespace EmployeeManagementSystem { // Application namespace
    public partial class IntroForm : Form { // Intro/splash form class
        public IntroForm() { // Constructor
            InitializeComponent(); // Initialize designer components
            DoubleBuffered = true; // Reduce flicker by using double buffering
            SetupHero(); // Configure the hero label and layout
        }

        private void SetupHero() { // Prepare central hero text label
            // Use the designer label inside pnlMain if it exists
            var container = pnlMain ?? (Control)this; // Choose panel if available, else form
            var lbl = label1 ?? container.Controls.OfType<Label>().FirstOrDefault(); // Reuse existing label or find first

            if (lbl == null) // If no label exists
            {
                lbl = new Label { Name = "label1" }; // Create a new label instance
                container.Controls.Add(lbl); // Add label to container
            }

            lbl.Text = "EMPLOYEE MANAGEMENT SYSTEM"; // Set hero text
            lbl.AutoSize = false; // Allow docking to control size
            lbl.Anchor = AnchorStyles.None; // Remove anchor constraints for full center behavior
            lbl.Dock = DockStyle.Fill; // Fill entire container area
            lbl.TextAlign = ContentAlignment.MiddleCenter; // Center text horizontally and vertically

            // Modern style
            lbl.Font = new Font("Segoe UI Semibold", 32f, FontStyle.Bold); // Set large bold font
            lbl.ForeColor = Color.FromArgb(34, 34, 34); // Dark modern text color

            container.BackColor = Color.White; // White background for contrast
            container.Padding = new Padding(0); // Remove any padding around label
        }
    }
}
