using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeManagementSystem {
    public partial class EmployeeForm : Form {

        EmployeeDataContext db = new EmployeeDataContext();
        public EmployeeForm() {
            InitializeComponent();
        }

        private void ResetContext() {
            try { db.Dispose(); } catch { }
            db = new EmployeeDataContext();
        }

        private void ApplyTotals() {
            // Sum of base salaries (unformatted values) then format
            var total = db.Employees.Any() ? db.Employees.Sum(e => e.EmpSal) : 0;
            labelTotalSalaryTitle.Visible = true;
            lblTotalSalary.Visible = true;
            lblTotalSalary.Text = CurrencyFormatter.Format(total) + " $"; // optional currency symbol
        }

        public void LoaddgvEmployee() {
            ResetContext(); // ensure fresh data after edits from other DataContext instances
            dgvEmployee.RowTemplate.Height = 50;
            string t = Application.StartupPath + @"\AddressImage\";
            dgvEmployee.DataSource = db.Employees.Select(p => new
            {
                p.EmpID,
                p.EmpGen,
                DepName = p.Department.DepName,
                p.EmpName,
                p.EmpDOB,
                p.EmpJDate,
                EmpSal = CurrencyFormatter.Format(p.EmpSal),
                EmpImage = Image.FromFile(t + p.EmpImage.ToString())
            });
            lbltotal.Text = dgvEmployee.RowCount.ToString();
            dgvEmployee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ApplyTotals();
        }

        private void EmployeeForm_Load(object sender, EventArgs e) {
            LoaddgvEmployee();
        }

        private void dgvEmployee_CellClick(object sender, DataGridViewCellEventArgs e) {
            int posrow = e.RowIndex;
            int poscol = e.ColumnIndex;
            if (posrow >= 0 && poscol >= 0)
            {
                if (dgvEmployee.Columns[poscol].Name == "Delete")
                {
                    if (MessageBox.Show("Are you sure delete this Employee ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Salary sal = db.Salaries.SingleOrDefault(m => m.EmployeeID == int.Parse(dgvEmployee["ID", posrow].Value.ToString()));
                        Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString()));
                        if (sal != null)
                            db.Salaries.DeleteOnSubmit(sal);
                        if (epl != null)
                            db.Employees.DeleteOnSubmit(epl);
                        db.SubmitChanges();
                    }
                    LoaddgvEmployee();
                } else if (dgvEmployee.Columns[poscol].Name == "Edit")
                {
                    Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString()));
                    UpdateEmployeeForm f = new UpdateEmployeeForm(epl);
                    // Pass the exact salary text shown in the grid so the popup matches what user sees
                    var salaryCellText = dgvEmployee["Column6", posrow].Value != null ? dgvEmployee["Column6", posrow].Value.ToString() : string.Empty;
                    f.PrefilledSalaryText = salaryCellText;
                    f.btnAdd.Enabled = false;
                    f.btnUpdate.Enabled = true;
                    f.ShowDialog();
                    LoaddgvEmployee(); // refresh after dialog
                } else if (dgvEmployee.Columns[poscol].Name == "imagecol")
                {
                    string addressimage = "";
                    string namefile = "";
                    OpenFileDialog dal = new OpenFileDialog();
                    dal.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp;)|*.jpg; *.jpeg; *.gif; *.bmp;";
                    if (dal.ShowDialog() == DialogResult.OK)
                    {
                        addressimage = dal.FileName;
                        namefile = Path.GetFileName(dal.FileName);
                        Employee epl = db.Employees.SingleOrDefault(m => m.EmpID == int.Parse(dgvEmployee["ID", posrow].Value.ToString()));
                        if (epl != null)
                        {
                            if (epl.EmpImage == namefile)
                            {
                                LoaddgvEmployee();
                                return;
                            }
                            if (epl.EmpImage != namefile && File.Exists(Application.StartupPath + "\\AddressImage\\" + namefile))
                            {
                                epl.EmpImage = namefile;
                                db.SubmitChanges();
                                LoaddgvEmployee();
                                return;
                            }
                            if (File.Exists(Application.StartupPath + "\\AddressImage\\" + namefile))
                            {
                                File.Delete(Application.StartupPath + "\\AddressImage\\" + namefile);
                            }
                            File.Copy(dal.FileName, Application.StartupPath + "\\AddressImage\\" + namefile);
                            epl.EmpImage = namefile;
                            db.SubmitChanges();
                        }
                    }
                    LoaddgvEmployee();
                }
            }
        }

        private void ptbAdd_Click(object sender, EventArgs e) {
            var f = new UpdateEmployeeForm();
            f.btnAdd.Enabled = true;
            f.btnUpdate.Enabled = false;
            f.ShowDialog();
            LoaddgvEmployee();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) {
            if (txtSearch.Text == "")
            {
                LoaddgvEmployee();
            } else
            {
                dgvEmployee.RowTemplate.Height = 50;
                string t = Application.StartupPath + @"\AddressImage\";
                dgvEmployee.DataSource = db.Employees.Where(m => m.EmpName.Contains(txtSearch.Text.Trim())).Select(p => new
                {
                    p.EmpID,
                    p.EmpGen,
                    DepName = p.Department.DepName,
                    p.EmpName,
                    p.EmpDOB,
                    p.EmpJDate,
                    EmpSal = CurrencyFormatter.Format(p.EmpSal),
                    EmpImage = Image.FromFile(t + p.EmpImage.ToString())
                });
                lbltotal.Text = dgvEmployee.RowCount.ToString();
                dgvEmployee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ApplyTotals();
            }
        }
    }
}
