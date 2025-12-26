# Employee Management System (EMS)

![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp&logoColor=white)
![Framework](https://img.shields.io/badge/.NET-Framework_4.7.2-512BD4)
![Database](https://img.shields.io/badge/Database-SQL_Server-CC2927?logo=microsoft-sql-server&logoColor=white)
![ORM](https://img.shields.io/badge/ORM-LINQ_to_SQL-blue)

A comprehensive desktop ERP solution designed to streamline HR operations. Built with **C# (Windows Forms)** and **SQL Server**, this application manages the full employee lifecycle—from onboarding to payroll processing and reporting.

It demonstrates enterprise-level features including **SMTP-based Two-Factor Authentication (OTP)** for password recovery and **RDLC Reporting** for payroll generation.

---

## 🏢 Key Features

### 🔐 Security & Authentication
* **Secure Login:** Encrypted credential verification against the SQL database.
* **Password Recovery:** Integrated **SMTP client** to send One-Time Passwords (OTP) via email (Gmail API) for account recovery.

### 👥 Core Management
* **Employee CRUD:** Complete lifecycle management (Hire, Update, Fire) with image blob storage.
* **Department Architecture:** Relational linking between employees and their operational departments.
* **Payroll System:** logic to calculate salaries based on pay periods and unpaid days.

### 📊 Analytics & Reporting
* **Interactive Dashboards:** Visual statistics using Chart controls to analyze salary distributions.
* **Automated Reporting:** Generates professional PDF payroll reports using **Microsoft RDLC Report Viewer**.

---

## 🛠️ Technical Architecture

### Database Schema (SQL Server)
The system is built on a normalized relational database:
* **`Department`** (1) ↔ (N) **`Employee`**
* **`Employee`** (1) ↔ (N) **`Salary`**
* **`Account`**: Stores administrative credentials.

### Data Access Layer
Instead of raw SQL strings, the application uses **LINQ to SQL** (`Employee.dbml`) to map database objects directly to C# classes, ensuring type safety and cleaner code.

```csharp
// Example: Using LINQ to fetch employees
var highEarners = from emp in context.Employees
                  where emp.EmpSal > 1000
                  select emp;
```

---

## 🚀 Installation & Setup
### Prerequisites
* Visual Studio 2022 (Desktop Development workload installed)
* SQL Server (Express, Developer, or Standard edition)
* .NET Framework 4.7.2

Step 1: Clone the Repository
```bash
git clone [https://github.com/TonyTheSlacker/EMPNhom4.git](https://github.com/TonyTheSlacker/EMPNhom4.git)
```
Step 2: Database Setup

  Open SQL Server Management Studio (SSMS).
  Create a new database named EmployeeManagementSystem.
  Run the provided script SQLQuery1.sql to generate the tables (Employee, Department, Salary, Account).

Step 3: Configure Connection

  Open EmployeeManagementSystem.sln in Visual Studio.
  Open App.config.
  Update the connectionString to match your local SQL instance (e.g., .\SQLEXPRESS):
  ```XML
  <add name="EmployeeManagementSystem.Properties.Settings.EmployeeManagementSystemConnectionString"
   connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=EmployeeManagementSystem;Integrated Security=True"
   providerName="System.Data.SqlClient" />
  ```

Step 4: Configure Email (Optional)
To test the "Forgot Password" feature:

  Open ForgetPasswordForm.cs.
  Locate the btnRequest_Click method.
  Replace the email credentials with your own (requires a Google App Password if using Gmail):
  ```csharp
    var from = new MailAddress("your-email@gmail.com");
    const string frompass = "your-16-digit-app-password";
  ```

Step 5: Run
Press F5 to build and launch the application

**Disclaimer: This is a portfolio project simulating an internal HR tool. Sensitive credentials (like email passwords) should be managed via Environment Variables in a production environment.**
