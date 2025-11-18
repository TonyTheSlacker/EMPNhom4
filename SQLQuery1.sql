CREATE TABLE Department (
    DepId INT PRIMARY KEY,
    DepName NVARCHAR(100) NOT NULL
)

CREATE TABLE Account (
    username VARCHAR(100) NOT NULL,
    password VARCHAR(100) NOT NULL,
    ID INT PRIMARY KEY,
    Email VARCHAR(500) NULL
)

CREATE TABLE Employee (
    EmpID INT PRIMARY KEY,
    EmpName NVARCHAR(100) NOT NULL,
    EmpGen VARCHAR(10) NOT NULL,
    EmpDep INT NOT NULL,
    EmpDOB DATE NOT NULL,
    EmpJDate DATE NOT NULL,
    EmpSal INT NOT NULL,
    EmpImage VARCHAR(500) NULL,
    CONSTRAINT FK_Employee_Department FOREIGN KEY (EmpDep) REFERENCES Department(DepId)
)

CREATE TABLE Salary (
    Scode INT PRIMARY KEY,
    EmployeeID INT NOT NULL,
    Period INT NOT NULL,
    Salary INT NOT NULL,
    Paydate DATE NOT NULL,
    EmployeeName NVARCHAR(50) NOT NULL,
    [From] DATE NOT NULL,
    [To] DATE NOT NULL,
    totalsal BIGINT NULL,
    UnpaidDays INT NULL,
    CONSTRAINT FK_Salary_Employee FOREIGN KEY (EmployeeID) REFERENCES Employee(EmpID)
)
