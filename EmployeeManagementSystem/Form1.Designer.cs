namespace EmployeeManagementSystem
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ptbAccount = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ptbLogout = new System.Windows.Forms.PictureBox();
            this.ptbSalary = new System.Windows.Forms.PictureBox();
            this.ptbDepartment = new System.Windows.Forms.PictureBox();
            this.ptbEmployee = new System.Windows.Forms.PictureBox();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ptbAccount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbLogout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbSalary)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbDepartment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbEmployee)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Orange;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 16.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.LightCyan;
            this.label1.Location = new System.Drawing.Point(28, 45);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(535, 113);
            this.label1.TabIndex = 0;
            this.label1.Text = "EMPLOYEE MANAGEMENT SYSTEM";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DarkOrange;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(0, 934);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1707, 80);
            this.panel2.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(1474, 133);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(129, 34);
            this.label7.TabIndex = 4;
            this.label7.Text = "LOGOUT";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(1088, 134);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 34);
            this.label6.TabIndex = 4;
            this.label6.Text = "SALARY";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(852, 134);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(188, 34);
            this.label5.TabIndex = 4;
            this.label5.Text = "DEPARTMENT";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label4.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(658, 134);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(154, 34);
            this.label4.TabIndex = 4;
            this.label4.Text = "EMPLOYEE";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkOrange;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.ptbAccount);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.ptbLogout);
            this.panel1.Controls.Add(this.ptbSalary);
            this.panel1.Controls.Add(this.ptbDepartment);
            this.panel1.Controls.Add(this.ptbEmployee);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1707, 205);
            this.panel1.TabIndex = 2;
            // 
            // ptbAccount
            // 
            this.ptbAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ptbAccount.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ptbAccount.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ptbAccount.Image = global::EmployeeManagementSystem.Properties.Resources.ic_fluent_patient_24_filled;
            this.ptbAccount.Location = new System.Drawing.Point(1292, 45);
            this.ptbAccount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ptbAccount.Name = "ptbAccount";
            this.ptbAccount.Size = new System.Drawing.Size(86, 84);
            this.ptbAccount.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ptbAccount.TabIndex = 7;
            this.ptbAccount.TabStop = false;
            this.ptbAccount.Click += new System.EventHandler(this.ptbAccount_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(1267, 134);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(155, 34);
            this.label8.TabIndex = 6;
            this.label8.Text = "ACCOUNT";
            // 
            // ptbLogout
            // 
            this.ptbLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ptbLogout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ptbLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ptbLogout.Image = global::EmployeeManagementSystem.Properties.Resources.ic_fluent_power_24_filled;
            this.ptbLogout.Location = new System.Drawing.Point(1486, 44);
            this.ptbLogout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ptbLogout.Name = "ptbLogout";
            this.ptbLogout.Size = new System.Drawing.Size(86, 84);
            this.ptbLogout.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ptbLogout.TabIndex = 5;
            this.ptbLogout.TabStop = false;
            this.ptbLogout.Click += new System.EventHandler(this.ptbLogout_Click);
            // 
            // ptbSalary
            // 
            this.ptbSalary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ptbSalary.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ptbSalary.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ptbSalary.Image = global::EmployeeManagementSystem.Properties.Resources.ic_fluent_currency_dollar_euro_24_regular;
            this.ptbSalary.Location = new System.Drawing.Point(1094, 45);
            this.ptbSalary.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ptbSalary.Name = "ptbSalary";
            this.ptbSalary.Size = new System.Drawing.Size(86, 84);
            this.ptbSalary.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ptbSalary.TabIndex = 5;
            this.ptbSalary.TabStop = false;
            this.ptbSalary.Click += new System.EventHandler(this.ptbSalary_Click);
            // 
            // ptbDepartment
            // 
            this.ptbDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ptbDepartment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ptbDepartment.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ptbDepartment.Image = global::EmployeeManagementSystem.Properties.Resources.ic_fluent_building_24_filled;
            this.ptbDepartment.Location = new System.Drawing.Point(895, 45);
            this.ptbDepartment.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ptbDepartment.Name = "ptbDepartment";
            this.ptbDepartment.Size = new System.Drawing.Size(86, 84);
            this.ptbDepartment.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ptbDepartment.TabIndex = 5;
            this.ptbDepartment.TabStop = false;
            this.ptbDepartment.Click += new System.EventHandler(this.ptbDepartment_Click);
            // 
            // ptbEmployee
            // 
            this.ptbEmployee.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ptbEmployee.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ptbEmployee.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ptbEmployee.Image = global::EmployeeManagementSystem.Properties.Resources.ic_fluent_person_24_filled;
            this.ptbEmployee.Location = new System.Drawing.Point(684, 45);
            this.ptbEmployee.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ptbEmployee.Name = "ptbEmployee";
            this.ptbEmployee.Size = new System.Drawing.Size(86, 84);
            this.ptbEmployee.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ptbEmployee.TabIndex = 5;
            this.ptbEmployee.TabStop = false;
            this.ptbEmployee.Click += new System.EventHandler(this.ptbEmployee_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 205);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1707, 729);
            this.pnlMain.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1707, 1014);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EMPLOYEE MANAGEMENT SYSTEM";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ptbAccount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbLogout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbSalary)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbDepartment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ptbEmployee)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox ptbLogout;
        private System.Windows.Forms.PictureBox ptbSalary;
        private System.Windows.Forms.PictureBox ptbDepartment;
        private System.Windows.Forms.PictureBox ptbEmployee;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.PictureBox ptbAccount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label1;
    }
}

