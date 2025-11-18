using System; // Base types and core utilities
using System.Drawing; // Drawing primitives (Color, Font, Bitmap)
using System.Drawing.Drawing2D; // Advanced drawing (smoothing modes)
using System.Linq; // LINQ extensions
using System.Windows.Forms; // Windows Forms UI

namespace EmployeeManagementSystem { // Application namespace
    public partial class AboutUsForm : Form { // About Us form class derived from Form
        public AboutUsForm() { // Constructor
            InitializeComponent(); // Initialize form controls created by designer

            // 1. Define the data in array
            var members = new[] // In-memory list of tmembers
            {
                new { FullName = "Châu Hoàn Thiện", StudentId = "49.01.103.077", Contribution = "Database, UI design, Form design" }, // Member 1
                new { FullName = "Đặng Minh Phúc", StudentId = "49.01.103.065", Contribution = "Form design, Testing" }, // Member 2
                new { FullName = "Trần Minh Mẫn", StudentId = "49.01.103.049", Contribution = "Report Document, Presentation" }, // Member 3
                new { FullName = "Nguyễn Lê Thanh Nhàn", StudentId = "49.01.103.055", Contribution = "Report Document, Presentation" } // Member 4
            };

            // 2. data to UI Controls[]
            var memberCards = members // Project each member into a UI panel (card)
                .Select(member => BuildCard(member.FullName, member.StudentId, member.Contribution)) // Create a card control per member
                .ToArray(); // Materialize into array for AddRange

            // 3. controls to panel
            this.flpCards.Controls.AddRange(memberCards); // Add all cards to the FlowLayoutPanel
        }

        private Control BuildCard(string fullName, string studentId, string contribution) { // Build a single member card panel
            var card = new Panel // Card container
            {
                Width = 128, // Fixed width
                Height = 230, // Fixed height
                Margin = new Padding(8), // Outer margin for flow layout
                BackColor = Color.White, // Card background color
                BorderStyle = BorderStyle.FixedSingle // Thin border
            };

            var pic = new PictureBox // Avatar picture
            {
                Size = new Size(68, 68), // Avatar size
                SizeMode = PictureBoxSizeMode.Zoom, // Scale image proportionally
                BackColor = Color.Gainsboro, // Placeholder background
                BorderStyle = BorderStyle.FixedSingle, // Border around avatar
                Image = CreateAvatar(fullName), // Generated avatar image based on name
                Location = new Point((card.Width - 68) / 2, 10) // Center horizontally, top padding 10
            };
            card.Controls.Add(pic); // Add avatar to card

            int y = pic.Bottom + 8; // Start Y for next control below avatar

            var lblName = new Label // Name label
            {
                Text = fullName, // Display full name
                Font = new Font("Century Gothic", 10.0f, FontStyle.Bold), // Bold font
                ForeColor = Color.DarkOrange, // Themed color
                AutoSize = true, // Grow to fit text
                MaximumSize = new Size(card.Width - 16, 0), // Wrap inside card
                Location = new Point(8, y), // Position with left padding
                Padding = new Padding(0, 2, 0, 0), // Small top padding
                UseCompatibleTextRendering = true // GDI+ text rendering for better quality
            };
            card.Controls.Add(lblName); // Add name label to card
            y = lblName.Bottom + 6; // Advance Y below name

            var lblId = new Label // Student ID label
            {
                Text = "ID: " + studentId, // Prefix and ID text
                Font = new Font("Century Gothic", 9.0f, FontStyle.Regular), // Regular font
                ForeColor = Color.DarkOrange, // Themed color
                AutoSize = true, // Auto size to content
                Location = new Point(8, y), // Position below name
                UseCompatibleTextRendering = true // Better text quality
            };
            card.Controls.Add(lblId); // Add ID label to card
            y = lblId.Bottom + 4; // Advance Y below ID

            var lblContribution = new Label // Contribution label
            {
                Text = contribution, // Contribution text
                Font = new Font("Century Gothic", 9.0f, FontStyle.Italic), // Italic style
                ForeColor = Color.DarkOrange, // Themed color
                AutoSize = true, // Auto size
                MaximumSize = new Size(card.Width - 16, 0), // Wrap inside card
                Location = new Point(8, y), // Position below ID
                UseCompatibleTextRendering = true // Better text quality
            };
            card.Controls.Add(lblContribution); // Add contribution to card

            return card; // Return constructed card control
        }

        private Image CreateAvatar(string name) { // Create a circular monogram avatar image
            int size = 90; // Bitmap size (square)
            var bmp = new Bitmap(size, size); // Create empty bitmap
            using (var g = Graphics.FromImage(bmp)) // Get drawing surface for bitmap
            {
                g.SmoothingMode = SmoothingMode.AntiAlias; // Smooth edges
                g.Clear(Color.WhiteSmoke); // Background color
                var rect = new Rectangle(0, 0, size - 1, size - 1); // Outer circle bounds
                using (var brush = new SolidBrush(Color.Gainsboro)) // Fill brush for circle
                    g.FillEllipse(brush, rect); // Draw filled circle
                using (var pen = new Pen(Color.DarkOrange, 3)) // Pen for circle outline
                    g.DrawEllipse(pen, rect); // Draw circle outline
                string text = string.IsNullOrEmpty(name) ? "?" : char.ToUpper(name[0]).ToString(); // First letter or '?'
                using (var f = new Font("Century Gothic", 28, FontStyle.Bold)) // Monogram font
                using (var txtBrush = new SolidBrush(Color.DarkOrange)) // Text color brush
                {
                    var sz = g.MeasureString(text, f); // Measure text size
                    g.DrawString(text, f, txtBrush, (size - sz.Width) / 2, (size - sz.Height) / 2); // Center text in circle
                }
            }
            return bmp; // Return generated bitmap
        }

        private void btnClose_Click(object sender, EventArgs e) { // Close button event handler
            this.Close(); // Close the About Us form
        }
    }
}