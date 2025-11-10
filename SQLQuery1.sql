SELECT * from Account
SELECT * from Employee
SELECT * from Department
SELECT * from Salary

INSERT INTO dbo.Employee
(
    EmpName,
    EmpGen,
    EmpDep,
    EmpDOB,
    EmpJDate,
    EmpSal,
    EmpImage
)
VALUES
(
    'Chau Hoan Thien', 
    'Male', 
    2, 
    '2222-10-21', 
    '2025-11-02', 
    5000000, 
    'avatar.jpg'
);