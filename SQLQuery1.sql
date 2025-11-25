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









INSERT INTO Student (Name, Birthday, Phone, Email)
VALUES 
(N'Nguyễn Văn An', '2003-05-15', '0901234567', 'an.nguyen@gmail.com'),
(N'Trần Thị Bích', '2004-08-20', '0912345678', 'bich.tran@gmail.com'),
(N'Lê Hoàng Nam', '2002-12-10', '0987654321', 'nam.le@gmail.com'),
(N'Phạm Minh Tuấn', '2005-01-30', '0933445566', 'tuan.pham@outlook.com'),
(N'Hoàng Thị Lan', '2003-11-25', '0945678901', 'lan.hoang@yahoo.com');
GO





-- =============================================
-- 1. Giáo viên dạy TIẾNG NHẬT
-- =============================================
-- Username là Email
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('akira.nguyen@email.com', '123456', 'Teacher'); 

DECLARE @UserID1 INT = SCOPE_IDENTITY();

INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Nguyễn Akira', N'Tiếng Nhật', '0901112233', 'akira.nguyen@email.com', @UserID1);


-- =============================================
-- 2. Giáo viên dạy TIẾNG PHÁP
-- =============================================
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('pierre.tran@email.com', '123456', 'Teacher');

DECLARE @UserID2 INT = SCOPE_IDENTITY();

INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Trần Pierre', N'Tiếng Pháp', '0912223344', 'pierre.tran@email.com', @UserID2);


-- =============================================
-- 3. Giáo viên dạy TIẾNG TRUNG
-- =============================================
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('mei.le@email.com', '123456', 'Teacher');

DECLARE @UserID3 INT = SCOPE_IDENTITY();

INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Lê Tiểu Mei', N'Tiếng Trung', '0983334455', 'mei.le@email.com', @UserID3);


-- =============================================
-- 4. Giáo viên dạy IELTS
-- =============================================
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('john.pham@email.com', '123456', 'Teacher');

DECLARE @UserID4 INT = SCOPE_IDENTITY();

INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Phạm John', N'IELTS', '0974445566', 'john.pham@email.com', @UserID4);


-- =============================================
-- 5. Giáo viên dạy TOEIC
-- =============================================
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('david.hoang@email.com', '123456', 'Teacher');

DECLARE @UserID5 INT = SCOPE_IDENTITY();

INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
VALUES (N'Hoàng David', N'TOEIC', '0935556677', 'david.hoang@email.com', @UserID5);



-- =============================================
-- TẠO TÀI KHOẢN ADMIN
-- =============================================

-- 1. Tạo User (Username là Email)
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('lebao1942005@gmail.com', '123', 'Admin'); -- Mật khẩu demo là 123

-- 2. Lấy UserID vừa tạo
DECLARE @AdminUserID INT = SCOPE_IDENTITY();

-- 3. Tạo thông tin chi tiết trong bảng Admin
INSERT INTO Admin (Name, Birthday, Phone, Email, UserID)
VALUES (N'Bao', '1990-01-01', '0999888777', 'admin@gmail.com', @AdminUserID);

GO