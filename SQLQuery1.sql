USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'EducationDB')
BEGIN
    ALTER DATABASE EducationDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EducationDB;
END
GO

-- T?o l?i database
CREATE DATABASE EducationDB;
GO

USE EducationDB;
GO

-------------------------------------------------------
-- B?ng Users
-------------------------------------------------------
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(30) NOT NULL CHECK (Role IN ('Admin','Teacher')),
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-------------------------------------------------------
-- B?ng Admin (1-1 Users)
-------------------------------------------------------
CREATE TABLE Admin (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Birthday DATE,
    Phone VARCHAR(20),
    Email NVARCHAR(200),
    UserID INT NOT NULL UNIQUE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-------------------------------------------------------
-- B?ng Teacher (1-1 Users)
-------------------------------------------------------
CREATE TABLE Teacher (
    TeacherID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Subject NVARCHAR(200),
    Phone VARCHAR(20),
    Email NVARCHAR(200),
    UserID INT NOT NULL UNIQUE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-------------------------------------------------------
-- B?ng Student
-------------------------------------------------------
CREATE TABLE Student (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Birthday DATE,
    Phone VARCHAR(20),
    Email NVARCHAR(200)
);

-------------------------------------------------------
-- B?ng Course
-------------------------------------------------------
CREATE TABLE Course (
    CourseID INT IDENTITY(1,1) PRIMARY KEY,
    CourseName NVARCHAR(200) NOT NULL,
    Certificate NVARCHAR(100),
    BaseFee DECIMAL(12,2) DEFAULT 0.00
);

-------------------------------------------------------
-- B?ng Class
-------------------------------------------------------
CREATE TABLE Class (
    ClassID INT IDENTITY(1,1) PRIMARY KEY,
    ClassName NVARCHAR(150) NOT NULL,
    CourseID INT NOT NULL,
    TeacherID INT NOT NULL,
    Schedule NVARCHAR(200),
    StartDate DATE,
    EndDate DATE,
    MaxStudents INT NOT NULL DEFAULT 20,
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID),
    FOREIGN KEY (TeacherID) REFERENCES Teacher(TeacherID)
);

-------------------------------------------------------
-- B?ng Enrollment
-------------------------------------------------------
CREATE TABLE Enrollment (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    ClassID INT NOT NULL,
    EnrollDate DATETIME DEFAULT GETUTCDATE(),
    Status NVARCHAR(30) DEFAULT 'Active',
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID),
    CONSTRAINT UQ_Enrollment UNIQUE(StudentID, ClassID) -- 1 h?c viên ??ng ký 1 l?p duy nh?t
);

-------------------------------------------------------
-- B?ng Tuition
-------------------------------------------------------
CREATE TABLE Tuition (
    TuitionID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    Amount DECIMAL(12,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Unpaid',
    PaidAt DATETIME,
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);

-------------------------------------------------------
-- B?ng ExamResult
-------------------------------------------------------
CREATE TABLE ExamResult (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    Score DECIMAL(5,2),
    GradingDate DATETIME,
    Note NVARCHAR(500),
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);

-------------------------------------------------------
-- B?ng Attendance
-------------------------------------------------------
CREATE TABLE Attendance (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    ClassID INT NOT NULL,
    SessionDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    Note NVARCHAR(500),
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID)
);

-------------------------------------------------------
-- B?ng Certificate
-------------------------------------------------------
CREATE TABLE Certificate (
    CertificateID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    CourseID INT NOT NULL,
    ResultID INT NOT NULL UNIQUE,
    IssueDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID),
    FOREIGN KEY (ResultID) REFERENCES ExamResult(ResultID)
);
