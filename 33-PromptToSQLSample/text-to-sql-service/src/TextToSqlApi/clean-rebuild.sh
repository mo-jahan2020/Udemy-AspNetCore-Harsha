#!/bin/bash
# ================================================================
# اسکریپت پاک‌سازی و rebuild پروژه
# اجرا: bash clean-rebuild.sh
# ================================================================

set -e

echo "🧹 پاک‌سازی bin و obj..."
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

echo "🧹 پاک‌سازی NuGet cache..."
dotnet nuget locals all --clear 2>/dev/null || true

echo "📦 Restore..."
dotnet restore

echo "🔨 Build..."
dotnet build --no-restore

echo "✅ پروژه با موفقیت build شد!"
