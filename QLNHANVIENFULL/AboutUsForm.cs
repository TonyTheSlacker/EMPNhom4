using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QLNHANVIENFULL {
    public partial class AboutUsForm : Form {
        public AboutUsForm() {
            InitializeComponent();
            // Build5 cards at runtime (keeps designer clean)
            AddMember("Châu Hoàn Thiện", "49.01.103.077", "Database, UI design, Form design");
            AddMember("Đặng Minh Phúc", "49.01.103.065", "Form design, Testing");
            AddMember("Trần Minh Mẫn", "49.01.103.049", "Report Document, Presentation");
            AddMember("Nguyễn Lê Thanh Nhàn", "49.01.103.055", "Report Document, Presentation");
        }

        private void AddMember(string fullName, string studentId, string contribution) {
            var card = BuildCard(fullName, studentId, contribution);
            this.flpCards.Controls.Add(card);
        }

        private Control BuildCard(string fullName, string studentId, string contribution) {
            var card = new Panel
            {
                Width = 128,
                Height = 230,
                Margin = new Padding(8),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pic = new PictureBox
            {
                Size = new Size(68, 68),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Image = CreateAvatar(fullName),
                Location = new Point((card.Width - 68) / 2, 10)
            };
            card.Controls.Add(pic);

            int y = pic.Bottom + 8;

            var lblName = new Label
            {
                Text = fullName,
                Font = new Font("Century Gothic", 10.0f, FontStyle.Bold),
                ForeColor = Color.DarkOrange,
                AutoSize = true,
                MaximumSize = new Size(card.Width - 16, 0), // wrap within card
                Location = new Point(8, y),
                Padding = new Padding(0, 2, 0, 0), // avoid ascender clipping
                UseCompatibleTextRendering = true
            };
            card.Controls.Add(lblName);
            y = lblName.Bottom + 6;

            var lblId = new Label
            {
                Text = "ID: " + studentId,
                Font = new Font("Century Gothic", 9.0f, FontStyle.Regular),
                ForeColor = Color.DarkOrange,
                AutoSize = true,
                Location = new Point(8, y),
                UseCompatibleTextRendering = true
            };
            card.Controls.Add(lblId);
            y = lblId.Bottom + 4;

            var lblContribution = new Label
            {
                Text = contribution,
                Font = new Font("Century Gothic", 9.0f, FontStyle.Italic),
                ForeColor = Color.DarkOrange,
                AutoSize = true,
                MaximumSize = new Size(card.Width - 16, 0),
                Location = new Point(8, y),
                UseCompatibleTextRendering = true
            };
            card.Controls.Add(lblContribution);

            return card;
        }

        private Image CreateAvatar(string name) {
            int size = 90;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.WhiteSmoke);
                var rect = new Rectangle(0, 0, size - 1, size - 1);
                using (var brush = new SolidBrush(Color.Gainsboro))
                    g.FillEllipse(brush, rect);
                using (var pen = new Pen(Color.DarkOrange, 3))
                    g.DrawEllipse(pen, rect);
                string text = string.IsNullOrEmpty(name) ? "?" : char.ToUpper(name[0]).ToString();
                using (var f = new Font("Century Gothic", 28, FontStyle.Bold))
                using (var txtBrush = new SolidBrush(Color.DarkOrange))
                {
                    var sz = g.MeasureString(text, f);
                    g.DrawString(text, f, txtBrush, (size - sz.Width) / 2, (size - sz.Height) / 2);
                }
            }
            return bmp;
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
