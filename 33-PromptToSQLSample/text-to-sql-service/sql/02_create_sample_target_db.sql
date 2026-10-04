/* ================================================================
   TextToSqlApi - Sample Target Database (اختیاری)
   فایل: 02_create_sample_target_db.sql
   ----------------------------------------------------------------
   یک دیتابیس نمونه برای تست تولید SQL.
   این دیتابیس فقط برای دمو و تست است؛ در محیط واقعی، شما
   دیتابیس هدف خودتان را با ConnectionString مربوطه معرفی می‌کنید.
   ================================================================ */

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SampleDb')
BEGIN
    CREATE DATABASE SampleDb;
    PRINT 'Database SampleDb created.';
END
GO

USE SampleDb;
GO

-- ----------------------------------------------------------------
-- جدول Customers
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        CustomerId     INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Customers] PRIMARY KEY,
        FirstName      NVARCHAR(100) NOT NULL,
        LastName       NVARCHAR(100) NOT NULL,
        Email          NVARCHAR(200) NULL,
        Country        NVARCHAR(100) NULL,
        CreatedAt      DATETIME2 NOT NULL CONSTRAINT [DF_Customers_CreatedAt] DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- ----------------------------------------------------------------
-- جدول Products
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId      INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Products] PRIMARY KEY,
        ProductName    NVARCHAR(200) NOT NULL,
        Category       NVARCHAR(100) NULL,
        UnitPrice      DECIMAL(18,2) NOT NULL,
        StockQuantity  INT NOT NULL
    );
END
GO

-- ----------------------------------------------------------------
-- جدول Orders
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId        INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Orders] PRIMARY KEY,
        CustomerId     INT NOT NULL,
        OrderDate      DATETIME2 NOT NULL,
        TotalAmount    DECIMAL(18,2) NOT NULL,
        CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers(CustomerId)
    );

    CREATE INDEX IX_Orders_CustomerId ON dbo.Orders(CustomerId);
    CREATE INDEX IX_Orders_OrderDate ON dbo.Orders(OrderDate);
END
GO

-- ----------------------------------------------------------------
-- جدول OrderItems
-- ----------------------------------------------------------------
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        OrderItemId    INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_OrderItems] PRIMARY KEY,
        OrderId        INT NOT NULL,
        ProductId      INT NOT NULL,
        Quantity       INT NOT NULL,
        UnitPrice      DECIMAL(18,2) NOT NULL,
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY (ProductId)
            REFERENCES dbo.Products(ProductId)
    );

    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
END
GO

-- ----------------------------------------------------------------
-- داده‌های نمونه
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers (FirstName, LastName, Email, Country) VALUES
        (N'علی', N'محمدی', N'ali@example.com', N'Iran'),
        (N'Mary', N'Johnson', N'mary@example.com', N'USA'),
        (N'Li', N'Wei', N'li@example.com', N'China'),
        (N'John', N'Smith', N'john@example.com', N'UK'),
        (N'فاطمه', N'احمدی', N'fateme@example.com', N'Iran');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (ProductName, Category, UnitPrice, StockQuantity) VALUES
        (N'Laptop Pro 15', N'Electronics', 1500.00, 50),
        (N'Wireless Mouse', N'Electronics', 25.00, 200),
        (N'Office Chair', N'Furniture', 180.00, 30),
        (N'Python Book', N'Books', 45.00, 100),
        (N'Coffee Mug', N'Accessories', 12.00, 300);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    INSERT INTO dbo.Orders (CustomerId, OrderDate, TotalAmount) VALUES
        (1, '2024-01-15', 1525.00),
        (2, '2024-02-20', 45.00),
        (3, '2024-03-10', 180.00),
        (4, '2024-04-05', 1525.00),
        (1, '2024-05-12', 57.00);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.OrderItems)
BEGIN
    INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES
        (1, 1, 1, 1500.00),
        (1, 2, 1, 25.00),
        (2, 4, 1, 45.00),
        (3, 3, 1, 180.00),
        (4, 1, 1, 1500.00),
        (4, 2, 1, 25.00),
        (5, 2, 1, 25.00),
        (5, 5, 1, 12.00),
        (5, 4, 1, 20.00);
END
GO

PRINT '=== Sample target database created with seed data ===';
GO
