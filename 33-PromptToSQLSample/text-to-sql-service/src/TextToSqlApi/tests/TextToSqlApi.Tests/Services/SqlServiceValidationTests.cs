using TextToSqlApi.Services;
using Xunit;

namespace TextToSqlApi.Tests.Services;

/// <summary>تست‌های استاتیک متد ValidateReadOnly در SqlService.</summary>
public class SqlServiceValidationTests
{
    [Theory]
    [InlineData("SELECT * FROM Customers")]
    [InlineData("SELECT TOP 100 * FROM Orders WHERE OrderDate > '2024-01-01'")]
    [InlineData("SELECT c.Name, COUNT(o.OrderId) FROM Customers c JOIN Orders o ON c.CustomerId = o.CustomerId GROUP BY c.Name")]
    [InlineData("WITH TopCustomers AS (SELECT * FROM Customers) SELECT * FROM TopCustomers")]
    [InlineData(";WITH CTE AS (SELECT * FROM Customers) SELECT * FROM CTE")]
    [InlineData("  SELECT 1  ")]
    public void ValidateReadOnly_Accepts_Valid_Select(string sql)
    {
        // Act + Assert: نباید استثنا پرتاب کند
        var ex = Record.Exception(() => SqlService.ValidateReadOnly(sql));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("DROP TABLE Customers")]
    [InlineData("DELETE FROM Customers WHERE 1=1")]
    [InlineData("UPDATE Customers SET Name='x'")]
    [InlineData("TRUNCATE TABLE Customers")]
    [InlineData("ALTER TABLE Customers ADD COLUMN x INT")]
    [InlineData("INSERT INTO Customers (Name) VALUES ('x')")]
    [InlineData("MERGE Customers AS t USING (SELECT 1 AS x) AS s ON 1=1 WHEN MATCHED THEN UPDATE SET t.Name='x';")]
    [InlineData("GRANT SELECT ON Customers TO public")]
    [InlineData("REVOKE SELECT ON Customers FROM public")]
    [InlineData("EXEC sp_help")]
    [InlineData("EXECUTE sp_help")]
    [InlineData("xp_cmdshell 'dir'")]
    [InlineData("BACKUP DATABASE SampleDb TO DISK='x.bak'")]
    [InlineData("RESTORE DATABASE SampleDb FROM DISK='x.bak'")]
    [InlineData("CREATE TABLE Hack(Id INT)")]
    [InlineData("SHUTDOWN")]
    public void ValidateReadOnly_Rejects_Dangerous(string sql)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => SqlService.ValidateReadOnly(sql));
        Assert.Contains("Blocked", ex.Message);
    }

    [Fact]
    public void ValidateReadOnly_Rejects_Empty_String()
    {
        Assert.Throws<InvalidOperationException>(() => SqlService.ValidateReadOnly(""));
        Assert.Throws<InvalidOperationException>(() => SqlService.ValidateReadOnly("   "));
    }

    [Fact]
    public void ValidateReadOnly_Rejects_Multiple_Statements()
    {
        var sql = "SELECT 1; DROP TABLE Customers;";
        var ex = Assert.Throws<InvalidOperationException>(() => SqlService.ValidateReadOnly(sql));
        Assert.Contains("semicolons", ex.Message);
    }

    [Fact]
    public void ValidateReadOnly_Strips_Comments_Before_Validation()
    {
        // کامنت‌ها باید قبل از اعتبارسنجی حذف شوند
        var sql = "/* DROP TABLE Customers */ SELECT * FROM Customers";
        var ex = Record.Exception(() => SqlService.ValidateReadOnly(sql));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateReadOnly_Catches_Dangerous_After_Select()
    {
        // یک SELECT معتبر به دنبالش DROP می‌آید (با ; ) - باید رد شود
        var sql = "SELECT * FROM Customers; DROP TABLE Customers;";
        var ex = Assert.Throws<InvalidOperationException>(() => SqlService.ValidateReadOnly(sql));
        // یا Multiple statements یا Blocked DROP
        Assert.True(ex.Message.Contains("semicolons") || ex.Message.Contains("Blocked"));
    }
}
