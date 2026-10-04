using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Options;
using OllamaSK.Api.Models;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace OllamaSK.Api.Services;

public interface IDocumentParser
{
    /// <summary>استخراج متن از فایل (متنی، Word یا Excel) با اعتبارسنجی و کوتاه‌سازی</summary>
    Task<string> ExtractAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>تشخیص نوع فایل: Text | Word | Excel | Unknown</summary>
    string DetectKind(string fileName);
}

/// <summary>
/// استخراج متن از فایل‌های پیوست:
///   - متنی  (.txt / .md / .csv / .json / .log)  → خواندن مستقیم
///   - ورد   (.docx)                             → با Open XML SDK
///   - اکسل  (.xlsx)                             → با ClosedXML
/// </summary>
public sealed class DocumentParser : IDocumentParser
{
    private static readonly HashSet<string> TextExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".md", ".log", ".csv", ".json" };

    private readonly FileProcessingOptions _options;
    private readonly ILogger<DocumentParser> _logger;

    public DocumentParser(IOptions<FileProcessingOptions> options, ILogger<DocumentParser> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string DetectKind(string fileName)
    {
        var ext = Path.GetExtension(fileName);

        if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            return "Word";

        if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return "Excel";

        if (TextExtensions.Contains(ext))
            return "Text";

        return "Unknown";
    }

    public async Task<string> ExtractAsync(IFormFile file, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName);
        var kind = DetectKind(file.FileName);

        if (kind == "Unknown")
            throw new NotSupportedException(
                $"فرمت «{ext}» پشتیبانی نمی‌شود. فرمت‌های مجاز: txt، md، csv، json، log، docx، xlsx");

        var maxBytes = _options.MaxFileSizeMB * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new InvalidDataException(
                $"حجم فایل بیش از حد مجاز است (حداکثر {_options.MaxFileSizeMB} مگابایت).");

        _logger.LogInformation("استخراج متن از فایل {File} ({Kind}، {Size} بایت)", file.FileName, kind, file.Length);

        await using var stream = file.OpenReadStream();
        var text = kind switch
        {
            "Word" => ExtractFromWord(stream),
            "Excel" => ExtractFromExcel(stream),
            _ => await ExtractFromTextAsync(stream, ct)
        };

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidDataException("هیچ متنی از فایل استخراج نشد؛ ممکن است فایل خالی باشد.");

        // کوتاه‌سازی متن برای جا شدن در کانتکست مدل
        if (text.Length > _options.MaxCharsPerDocument)
            text = text[.._options.MaxCharsPerDocument] + "\n…[متن سند به‌دلیل طول زیاد کوتاه شد]";

        return text;
    }

    // ─────────────────────────── فایل‌های متنی ───────────────────────────

    private static async Task<string> ExtractFromTextAsync(Stream stream, CancellationToken ct)
    {
        // detectEncodingFromByteOrderMarks: فایل‌های یونیکد با BOM را هم درست می‌خواند
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(ct);
    }

    // ─────────────────────────── فایل Word (docx) ───────────────────────────

    private static string ExtractFromWord(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Descendants<W.Paragraph>())
        {
            // InnerText متن پاراگراف است؛ پاراگراف‌های داخل جدول‌ها هم شامل می‌شوند
            var line = paragraph.InnerText?.Trim();
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }

        return sb.ToString();
    }

    // ─────────────────────────── فایل Excel (xlsx) ───────────────────────────

    private static string ExtractFromExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sb = new StringBuilder();

        foreach (var sheet in workbook.Worksheets)
        {
            sb.AppendLine($"--- برگه: {sheet.Name} ---");

            // حداکثر ۲۰۰۰ ردیف اول برای جلوگیری از پر شدن کانتکست
            foreach (var row in sheet.RowsUsed().Take(2000))
            {
                var cells = row.CellsUsed()
                    .Select(c => c.Value.ToString()?.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v));

                var line = string.Join(" | ", cells);
                if (!string.IsNullOrWhiteSpace(line))
                    sb.AppendLine(line);
            }
        }

        return sb.ToString();
    }
}
