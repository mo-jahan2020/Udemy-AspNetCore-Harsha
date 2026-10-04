# ═══════════════════════════════════════════════════════════════
#  download-bootstrap.ps1 — دانلود Bootstrap 5.3.3 RTL + فونت وزیرمتن
#  اجرا:  در پوشه پروژه، PowerShell را باز کنید و تایپ کنید:
#         .\scripts\download-bootstrap.ps1
#  (برای اجازه اجرا ممکن است لازم باشد: Set-ExecutionPolicy -Scope Process Bypass)
# ═══════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\wwwroot\lib"

$files = @(
    @{ Url = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css";      Out = "bootstrap\dist\css\bootstrap.rtl.min.css" },
    @{ Url = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js";     Out = "bootstrap\dist\js\bootstrap.bundle.min.js" },
    @{ Url = "https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/fonts/webfonts/Vazirmatn-Regular.woff2"; Out = "vazirmatn\fonts\Vazirmatn-Regular.woff2" },
    @{ Url = "https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/fonts/webfonts/Vazirmatn-Bold.woff2";    Out = "vazirmatn\fonts\Vazirmatn-Bold.woff2" }
)

foreach ($f in $files) {
    $dest = Join-Path $root $f.Out
    $dir  = Split-Path $dest -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Write-Host "دانلود $($f.Url)" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $f.Url -OutFile $dest -UseBasicParsing
    Write-Host "  → $dest" -ForegroundColor Green
}

Write-Host "`n✅ همه فایل‌ها دانلود شدند. برنامه کاملاً آفلاین اجرا می‌شود." -ForegroundColor Green
Write-Host "نکته امنیتی: Bootstrap آفلاین یعنی بدون CDN و بدون نیاز به SRI (بند V15.1.2)" -ForegroundColor Yellow
