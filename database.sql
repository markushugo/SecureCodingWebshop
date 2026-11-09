USE master;
GO
IF DB_ID(N'SecureCodingWebshop') IS NULL CREATE DATABASE SecureCodingWebshop;
GO
USE SecureCodingWebshop;
GO

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
CREATE TABLE dbo.Product (
    Id int IDENTITY PRIMARY KEY,
    Name nvarchar(120) NOT NULL,
    Description nvarchar(1000) NOT NULL,
    Price decimal(18,2) NOT NULL CHECK (Price > 0),
    Stock int NOT NULL CHECK (Stock >= 0),
    IsActive bit NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT 1
);

IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
CREATE TABLE dbo.AppUser (
    Id int IDENTITY PRIMARY KEY,
    Email nvarchar(254) NOT NULL,
    PasswordHash nvarchar(500) NOT NULL,
    Role nvarchar(20) NOT NULL CONSTRAINT DF_AppUser_Role DEFAULT 'Customer',
    FailedLoginCount int NOT NULL CONSTRAINT DF_AppUser_Failed DEFAULT 0,
    LockedUntilUtc datetimeoffset NULL,
    CONSTRAINT UQ_AppUser_Email UNIQUE (Email),
    CONSTRAINT CK_AppUser_Role CHECK (Role IN ('Customer', 'Admin'))
);

IF OBJECT_ID(N'dbo.ShopOrder', N'U') IS NULL
CREATE TABLE dbo.ShopOrder (
    Id int IDENTITY PRIMARY KEY,
    UserId int NOT NULL REFERENCES dbo.AppUser(Id),
    CreatedUtc datetimeoffset NOT NULL CONSTRAINT DF_Order_Created DEFAULT SYSUTCDATETIME(),
    Total decimal(18,2) NOT NULL CHECK (Total >= 0)
);

IF OBJECT_ID(N'dbo.OrderLine', N'U') IS NULL
CREATE TABLE dbo.OrderLine (
    Id int IDENTITY PRIMARY KEY,
    OrderId int NOT NULL REFERENCES dbo.ShopOrder(Id),
    ProductId int NOT NULL REFERENCES dbo.Product(Id),
    ProductName nvarchar(120) NOT NULL,
    UnitPrice decimal(18,2) NOT NULL CHECK (UnitPrice > 0),
    Quantity int NOT NULL CHECK (Quantity BETWEEN 1 AND 20)
);

IF OBJECT_ID(N'dbo.AuditEvent', N'U') IS NULL
CREATE TABLE dbo.AuditEvent (
    Id bigint IDENTITY PRIMARY KEY,
    UserId int NULL REFERENCES dbo.AppUser(Id),
    EventType nvarchar(60) NOT NULL,
    Succeeded bit NOT NULL,
    Details nvarchar(300) NULL,
    CreatedUtc datetimeoffset NOT NULL CONSTRAINT DF_Audit_Created DEFAULT SYSUTCDATETIME()
);

IF NOT EXISTS (SELECT 1 FROM dbo.Product)
INSERT dbo.Product (Name, Description, Price, Stock) VALUES
(N'Mekanisk tastatur', N'Kompakt nordisk tastatur til skole og gaming.', 699.00, 15),
(N'Trådløs mus', N'Enkel ergonomisk mus med USB-modtager.', 349.00, 25),
(N'USB-C hub', N'Hub med HDMI, netværk og tre USB-porte.', 449.00, 12),
(N'Laptopstander', N'Justerbar stander i aluminium.', 299.00, 20);
GO

-- Opret først en almindelig bruger gennem websiden. Gør den derefter til demo-admin:
-- UPDATE dbo.AppUser SET Role = 'Admin' WHERE Email = 'admin@example.local';
