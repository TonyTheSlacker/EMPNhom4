using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.Linq; // added for RefreshMode

namespace EmployeeManagementSystem {
    public partial class SalaryForm : Form {
        EmployeeDataContext db = new EmployeeDataContext();
        public SalaryForm() {
            InitializeComponent();
        }

        private void ResetContext() { // recreate context to avoid stale tracked entities
            try { db.Dispose(); } catch { }
            db = new EmployeeDataContext();
        }
        private void RefreshTrackedRows() { // fallback if you prefer not recreating
            try { db.Refresh(RefreshMode.OverwriteCurrentValues, db.Salaries); } catch { }
        }

        private int CalcMonths(DateTime from, DateTime to) {
            from = from.Date;
            to = to.Date;
            if (to < from)
                return 0;
            int months = (to.Year - from.Year) * 12 + (to.Month - from.Month);
            if (to.Day < from.Day)
                months--; // incomplete last month
            if (months < 1)
                months = 1; // minimum 1
            return months;
        }

        // Long version to avoid overflow (support big totals) -------------------------------------------------
        private long ComputeNetTotalLong(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) {
            var f = from.Date;
            var t = to.Date;
            if (t < f || monthlySalary <= 0)
                return 0L;
            int months = CalcMonths(f, t);
            if (months <= 0)
                return 0L;
            // Use long for gross
            long gross = (long)months * (long)monthlySalary;
            int rangeDays = (t - f).Days + 1;
            int unpaidDays = Math.Min(Math.Max(unpaidDaysNullable.GetValueOrDefault(0), 0), Math.Max(rangeDays, 0));
            // Use decimal daily rate but convert to long after multiplication
            decimal dailyRate = Math.Ceiling((decimal)monthlySalary / 30m);
            decimal deduction = unpaidDays * dailyRate;
            decimal netDec = ((decimal)gross) - deduction;
            if (netDec < 0)
                netDec = 0m;
            // Clamp to long range
            if (netDec > long.MaxValue)
                return long.MaxValue;
            return (long)netDec;
        }

        // Legacy int version - now wraps long version and clamps to int for storage --------------------------
        private int ComputeNetTotal(int monthlySalary, DateTime from, DateTime to, int? unpaidDaysNullable) {
            long v = ComputeNetTotalLong(monthlySalary, from, to, unpaidDaysNullable);
            if (v > int.MaxValue)
                return int.MaxValue;
            return (int)v;
        }

        private void RecalculateStoredPeriods() {
            bool anyChange = false;
            foreach (var s in db.Salaries)
            {
                int newMonths = CalcMonths(s.From, s.To);
                long newTotal = ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays);
                if (s.Period != newMonths)
                {
                    s.Period = newMonths;
                    anyChange = true;
                }
                if (s.totalsal.GetValueOrDefault() != newTotal)
                {
                    s.totalsal = newTotal;
                    anyChange = true;
                }
            }
            if (anyChange)
            {
                try
                {
                    db.SubmitChanges();
                } catch { }
            }
        }

        // Bind grid using existing designer columns (AutoGenerateColumns=false)
        public void LoadSalary() {
            try
            {
                ResetContext(); // ensure fresh data
                RecalculateStoredPeriods();
                dgvSalary.AutoGenerateColumns = false;
                var rows = db.Salaries.ToList(); // materialize to avoid provider overflows
                dgvSalary.DataSource = rows
                    .Select(p => new
                    {
                        EmpID = p.EmployeeID,
                        EmpName = p.EmployeeName, // use stored column, avoids null navigation
                        Scode = p.Scode,
                        Salary1 = CurrencyFormatter.Format(p.Salary1),
                        Period = p.Period,
                        From = p.From,
                        To = p.To,
                        Paydate = p.Paydate,
                        // Display full long value (recomputed) instead of persisted clamped int
                        totalsal = CurrencyFormatter.Format(ComputeNetTotalLong(p.Salary1, p.From, p.To, p.UnpaidDays)),
                        UnpaidDays = p.UnpaidDays
                    })
                    .ToList();
                // Sum using long recomputation to avoid int overflow
                long totalAll = rows.Sum(s => ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays));
                lbltotal.Text = CurrencyFormatter.Format(totalAll) + "$";
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to load salaries: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SalaryForm_Load(object sender, EventArgs e) {

            LoadSalary();
        }
        private void dgvSalary_CellClick(object sender, DataGridViewCellEventArgs e) {
            int posrow = e.RowIndex;
            int poscol = e.ColumnIndex;
            if (posrow >= 0 && poscol >= 0)
            {
                if (dgvSalary.Columns[poscol].Name == "Delete") // Delete
                {
                    if (MessageBox.Show("Are you sure delete this Salary ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            Salary sal = db.Salaries.SingleOrDefault(m => m.Scode == int.Parse(dgvSalary["IDSal", posrow].Value.ToString()));
                            if (sal != null)
                            {
                                db.Salaries.DeleteOnSubmit(sal);
                                db.SubmitChanges();
                            }
                        } catch (Exception ex)
                        {
                            MessageBox.Show("Delete failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        LoadSalary();
                    }

                } else if (dgvSalary.Columns[poscol].Name == "Edit") //Update
                {
                    try
                    {
                        Salary epl = db.Salaries.SingleOrDefault(m => m.Scode == int.Parse(dgvSalary["IDSal", posrow].Value.ToString()));
                        if (epl == null)
                            return;
                        UpdateSalaryForm f = new UpdateSalaryForm(epl) { btnAdd = { Enabled = false }, btnUpdate = { Enabled = true } };
                        f.ShowDialog();
                    } catch (Exception ex)
                    {
                        MessageBox.Show("Edit failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    LoadSalary();
                }

            }
        }
        private void ptbAdd_Click(object sender, EventArgs e) {
            var f = new UpdateSalaryForm();
            f.btnAdd.Enabled = true;
            f.btnUpdate.Enabled = false;
            f.ShowDialog();
            LoadSalary();
        }
        private void txtSearch_TextChanged(object sender, EventArgs e) {
            if (txtSearch.Text == "")
            {
                LoadSalary();
                return;
            }
            string term = txtSearch.Text.Trim();
            try
            {
                ResetContext();
                RecalculateStoredPeriods();
                dgvSalary.AutoGenerateColumns = false;
                var rows = db.Salaries.Where(m => (m.EmployeeName ?? string.Empty).Contains(term) || m.EmployeeID.ToString().Contains(term)).ToList();
                dgvSalary.DataSource = rows
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
                    .ToList();
                long totalAll = rows.Sum(s => ComputeNetTotalLong(s.Salary1, s.From, s.To, s.UnpaidDays));
                lbltotal.Text = CurrencyFormatter.Format(totalAll) + "$";
            } catch (Exception ex)
            {
                MessageBox.Show("Search failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            ReportForm reportForm = new ReportForm();
            reportForm.ShowDialog();
        }

        private void dgvSalary_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }
    }
}
