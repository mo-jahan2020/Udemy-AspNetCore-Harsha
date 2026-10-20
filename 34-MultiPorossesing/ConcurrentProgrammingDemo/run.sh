#!/bin/bash
# ----------------------------------------------------------------------------
#  run.sh - اسکریپت اجرای پروژه آموزشی اجرای همزمان در C# 10
# ----------------------------------------------------------------------------
#  نحوه استفاده:
#    ./run.sh        # منوی تعاملی - عدد دلخواه را وارد کنید
#    ./run.sh 5      # اجرای همه مثال‌ها
#    ./run.sh 1      # فقط Asynchronous
#    ./run.sh 2      # فقط Multithreading
#    ./run.sh 3      # فقط Parallel Processing
#    ./run.sh 4      # فقط Advanced Patterns
# ----------------------------------------------------------------------------
set -e

# تعیین مسیر dotnet نصب‌شده محلی
DOTNET_DIR="/home/z/my-project/dotnet"
export PATH="$DOTNET_DIR:$PATH"

# مسیر پروژه
PROJECT_DIR="/home/z/my-project/download/ConcurrentProgrammingDemo/ConcurrentProgrammingDemo"
cd "$PROJECT_DIR"

# کامپایل
echo "در حال کامپایل..."
dotnet build --nologo -v q 2>&1 | tail -5

# اجرا
if [ -n "$1" ]; then
    echo "$1" | dotnet run --no-build
else
    dotnet run --no-build
fi
