using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    public partial class UpdateEmployeeForm : Form {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        EmployeeDataContext db = new EmployeeDataContext();
        Employee employee = null;

        // Allow caller to pass the exact salary text from the grid (already formatted)
        public string PrefilledSalaryText {
            get; set;
        }

        public UpdateEmployeeForm(Employee e = null) {
            employee = e;
            InitializeComponent();

        }
        void LoadDataDep() {
            cbbDepartment.Items.Clear();
            cbbDepartment.DataSource = db.Departments;
            cbbDepartment.DisplayMember = "DepName";
            cbbDepartment.ValueMember = "DepId";
        }
        private void ptbClose_Click(object sender, EventArgs e) {
            this.Dispose();
        }

        private void pnltop_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || cbbgender.Text == "")
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            } else
            {
                Employee epl = new Employee();
                epl.EmpName = txtName.Text.Trim();
                epl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text);
                epl.EmpDOB = dtpkDOB.Value;
                epl.EmpJDate = dtpkJDate.Value;
                epl.EmpGen = cbbgender.SelectedItem.ToString();
                epl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString());
                epl.EmpImage = "avatar.jpg";
                db.Employees.InsertOnSubmit(epl);
                db.SubmitChanges();
                MessageBox.Show("Added sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
            }
        }

        // Helper logic copied from SalaryForm to keep salary rows consistent when employee base salary changes
        private int CalcMonths(DateTime from, DateTime to) {
            from = from.Date;
            to = to.Date;
            if (to < from)
                return 0;
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month);
            if (to.Day < from.Day)
                months--;
            if (months < 1)
                months = 1;
            return months;
        }
        private int ComputeNetTotal(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) {
            var f = from.Date;
            var t = to.Date;
            if (t < f || monthlySalary <= 0)
                return 0;
            int months = CalcMonths(f, t);
            if (months <= 0)
                return 0;
            decimal gross = months * (decimal)monthlySalary;
            int rangeDays = (t - f).Days + 1;
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0));
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m);
            decimal net = gross - (unpaidDays * dailyRate);
            if (net < 0)
                net = 0;
            return (int)net;
        }

        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) {
            var f = from.Date;
            var t = to.Date;
            if (t < f || monthlySalary <= 0)
                return 0L;
            int months = CalcMonths(f, t);
            if (months <= 0)
                return 0L;
            long gross = (long)months * monthlySalary;
            int rangeDays = (t - f).Days + 1;
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0));
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m);
            decimal netDec = (decimal)gross - (unpaidDays * dailyRate);
            if (netDec < 0)
                netDec = 0m;
            if (netDec > long.MaxValue)
                return long.MaxValue;
            return (long)netDec;
        }

        private void txtSalary_KeyPress(object sender, KeyPressEventArgs e) {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UpdateEmployeeForm_Load(object sender, EventArgs e) {
            LoadDataDep();
            if (employee != null)
            {
                txtName.Text = employee.EmpName;
                // Prefer the text shown in the grid if provided (ensures exact prefill like400.000)
                if (!string.IsNullOrWhiteSpace(PrefilledSalaryText))
                    txtSalary.Text = PrefilledSalaryText;
                else
                    txtSalary.Text = CurrencyFormatter.Format(employee.EmpSal);
                cbbgender.Text = employee.EmpGen.ToString();
                // Select department by value to ensure SelectedValue is valid later
                try
                {
                    cbbDepartment.SelectedValue = employee.EmpDep;
                } catch { cbbDepartment.Text = employee.Department.DepName.ToString().Trim(); }
                // Set DateTimePicker Value instead of Text
                dtpkDOB.Value = employee.EmpDOB;
                dtpkJDate.Value = employee.EmpJDate;
            }
        }

        private void label6_Click(object sender, EventArgs e) {

        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || string.IsNullOrEmpty(cbbgender.Text) || string.IsNullOrEmpty(cbbDepartment.Text))
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                Employee empl = db.Employees.SingleOrDefault(m => m.EmpID == employee.EmpID);
                empl.EmpName = txtName.Text.Trim();
                empl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text);
                empl.EmpDOB = dtpkDOB.Value;
                empl.EmpJDate = dtpkJDate.Value;
                empl.EmpGen = cbbgender.SelectedItem.ToString();
                empl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString());
                // Propagate new base salary to existing salary records (business choice)
                var salaries = db.Salaries.Where(s => s.EmployeeID == empl.EmpID).ToList();
                foreach (var s in salaries)
                {
                    s.Salary1 = empl.EmpSal; // update base monthly salary
                    // Align salary period start with employee join date so months reflect Join Date change
                    s.From = empl.EmpJDate;
                    s.Period = CalcMonths(s.From, s.To);
                    long newTotal = ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays);
                    s.totalsal = (int)Math.Min(newTotal, (long)int.MaxValue);
                }
                db.SubmitChanges();
                MessageBox.Show("Updated sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtSalary.Text) || cbbgender.Text == "")
            {
                MessageBox.Show("Miss Data", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            } else
            {
                Employee epl = new Employee();
                epl.EmpName = txtName.Text.Trim();
                epl.EmpSal = CurrencyFormatter.ParseToInt(txtSalary.Text);
                epl.EmpDOB = dtpkDOB.Value;
                epl.EmpJDate = dtpkJDate.Value;
                epl.EmpGen = cbbgender.SelectedItem.ToString();
                epl.EmpDep = int.Parse(cbbDepartment.SelectedValue.ToString());
                epl.EmpImage = "avatar.jpg";
                db.Employees.InsertOnSubmit(epl);
                db.SubmitChanges();
                MessageBox.Show("Added sucessfully !", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
            }
        }
    }
}
