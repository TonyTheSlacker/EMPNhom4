using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLNHANVIENFULL {
    public partial class IntroForm : Form {
        public IntroForm() {
            InitializeComponent();
            DoubleBuffered = true;
            SetupHero();
        }

        private void SetupHero() {
            // Use the designer label inside pnlMain if it exists
            var container = pnlMain ?? (Control)this;
            var lbl = label1 ?? container.Controls.OfType<Label>().FirstOrDefault();

            if (lbl == null)
            {
                lbl = new Label { Name = "label1" };
                container.Controls.Add(lbl);
            }

            lbl.Text = "EMPLOYEE MANAGEMENT SYSTEM";
            lbl.AutoSize = false;                    // allow Dock to size it
            lbl.Anchor = AnchorStyles.None;          // ignore designer anchors
            lbl.Dock = DockStyle.Fill;               // take all space in white area
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            // Modern style
            lbl.Font = new Font("Segoe UI Semibold", 32f, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(34, 34, 34);

            container.BackColor = Color.White;
            container.Padding = new Padding(0);
        }
    }
}
