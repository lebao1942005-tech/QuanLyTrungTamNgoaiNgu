USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'EducationDB')
BEGIN
    ALTER DATABASE EducationDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EducationDB;
END
GO

-- Tạo lại database
CREATE DATABASE EducationDB;
GO

USE EducationDB;
GO

-------------------------------------------------------
-- Bảng Users
-------------------------------------------------------
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(30) NOT NULL CHECK (Role IN ('Admin','Teacher')),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    TeacherID INT NULL
);

-------------------------------------------------------
-- Bảng Admin (1-1 Users)
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
-- Bảng Teacher (1-1 Users)
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
-- Bảng Student
-------------------------------------------------------
CREATE TABLE Student (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Birthday DATE,
    Phone VARCHAR(20),
    Email NVARCHAR(200)
);

-------------------------------------------------------
-- Bảng Course
-------------------------------------------------------
CREATE TABLE Course (
    CourseID INT IDENTITY(1,1) PRIMARY KEY,
    CourseName NVARCHAR(200) NOT NULL,
    DurationMonths INT NOT NULL,   -- số tháng học
    BaseFee DECIMAL(12,2) DEFAULT 0.00
);


-------------------------------------------------------
-- Bảng Class
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
-- Bảng Enrollment
-------------------------------------------------------
CREATE TABLE Enrollment (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT NOT NULL,
    ClassID INT NOT NULL,
    EnrollDate DATETIME DEFAULT GETUTCDATE(),
    Status NVARCHAR(30) DEFAULT 'Active',
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID),
    CONSTRAINT UQ_Enrollment UNIQUE(StudentID, ClassID)
);

-------------------------------------------------------
-- Bảng Tuition (Amount tự động lấy BaseFee từ Course)
-------------------------------------------------------
CREATE TABLE Tuition (
    TuitionID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL UNIQUE,
    Amount DECIMAL(12,2) NULL,
    Status NVARCHAR(20) DEFAULT 'Unpaid',
    PaidAt DATETIME,
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);
GO

-- Trigger tự động gán Amount = BaseFee
CREATE TRIGGER trg_Tuition_Insert
ON Tuition
AFTER INSERT
AS
BEGIN
    UPDATE t
    SET t.Amount = c.BaseFee
    FROM Tuition t
    JOIN Enrollment e ON t.EnrollmentID = e.EnrollmentID
    JOIN Class cl ON e.ClassID = cl.ClassID
    JOIN Course c ON cl.CourseID = c.CourseID
    WHERE t.Amount IS NULL;
END;
GO

CREATE TRIGGER trg_Class_Insert
ON Class
AFTER INSERT
AS
BEGIN
    UPDATE cl
    SET cl.EndDate = DATEADD(MONTH, c.DurationMonths, cl.StartDate)
    FROM Class cl
    JOIN Course c ON cl.CourseID = c.CourseID
    JOIN inserted i ON cl.ClassID = i.ClassID
    WHERE cl.EndDate IS NULL;
END;
GO


-------------------------------------------------------
-- Bảng ExamResult
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
-- Bảng Attendance
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
-- Dữ liệu mẫu Student
-------------------------------------------------------
INSERT INTO Student (Name, Birthday, Phone, Email)
VALUES 
(N'Nguyễn Văn An', '2003-05-15', '0901234567', 'an.nguyen@gmail.com'),
(N'Trần Thị Bích', '2004-08-20', '0912345678', 'bich.tran@gmail.com'),
(N'Lê Hoàng Nam', '2002-12-10', '0987654321', 'nam.le@gmail.com'),
(N'Phạm Minh Tuấn', '2005-01-30', '0933445566', 'tuan.pham@outlook.com'),
(N'Hoàng Thị Lan', '2003-11-25', '0945678901', 'lan.hoang@yahoo.com');
GO

-------------------------------------------------------
-- Dữ liệu mẫu Teacher + Users
-------------------------------------------------------
-- Giáo viên Tiếng Nhật
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('akira.nguyen@email.com', '123456', 'Teacher');  
DECLARE @UserID1 INT = SCOPE_IDENTITY();
INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Nguyễn Akira', N'Tiếng Nhật', '0901112233', 'akira.nguyen@email.com', @UserID1);

-- Giáo viên Tiếng Pháp
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('pierre.tran@email.com', '123456', 'Teacher');  
DECLARE @UserID2 INT = SCOPE_IDENTITY();
INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Trần Pierre', N'Tiếng Pháp', '0912223344', 'pierre.tran@email.com', @UserID2);

-- Giáo viên Tiếng Trung
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('mei.le@email.com', '123456', 'Teacher');  
DECLARE @UserID3 INT = SCOPE_IDENTITY();
INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Lê Tiểu Mei', N'Tiếng Trung', '0983334455', 'mei.le@email.com', @UserID3);

-- Giáo viên IELTS
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('john.pham@email.com', '123456', 'Teacher');  
DECLARE @UserID4 INT = SCOPE_IDENTITY();
INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Phạm John', N'IELTS', '0974445566', 'john.pham@email.com', @UserID4);

-- Giáo viên TOEIC
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('david.hoang@email.com', '123456', 'Teacher');  
DECLARE @UserID5 INT = SCOPE_IDENTITY();
INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Hoàng David', N'TOEIC', '0935556677', 'david.hoang@email.com', @UserID5);

-------------------------------------------------------
-- Admin
-------------------------------------------------------
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('lebao1942005@gmail.com', '123456', 'Admin');  
DECLARE @AdminUserID INT = SCOPE_IDENTITY();
INSERT INTO Admin (Name, Birthday, Phone, Email, UserID)
VALUES (N'Bao', '1990-01-01', '0999888777', 'lebao1942005@gmail.com', @AdminUserID);
GO