using TextToSqlApi.Services;
using Xunit;

namespace TextToSqlApi.Tests.Services;

/// <summary>تست‌های متد ExtractSql در OllamaService.</summary>
public class OllamaServiceExtractTests
{
    [Theory]
    [InlineData("SELECT TOP 100 * FROM Customers", "SELECT TOP 100 * FROM Customers")]
    [InlineData("```sql\nSELECT TOP 100 * FROM Customers\n```", "SELECT TOP 100 * FROM Customers")]
    [InlineData("```\nSELECT 1\n```", "SELECT 1")]
    [InlineData("```sql\nSELECT * FROM Orders\n```", "SELECT * FROM Orders")]
    [InlineData("  SELECT 1  ", "SELECT 1")]
    public void ExtractSql_Returns_Clean_Sql(string input, string expected)
    {
        var result = OllamaService.ExtractSql(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractSql_Handles_Empty(string input)
    {
        var result = OllamaService.ExtractSql(input);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractSql_Removes_Multiple_Fences()
    {
        // اگر خروجی حاوی چندین ``` باشد
        var input = "Here is the SQL:\n```sql\nSELECT 1\n```\nDone.";
        var result = OllamaService.ExtractSql(input);
        // این مورد چون با ``` شروع نمی‌شود، فقط fences حذف می‌شود
        Assert.Contains("SELECT 1", result);
    }
}
