"""تست‌های ساده برای Vanna Microservice با pytest-fastapi."""

import sys
import os
import pytest

# اضافه کردن مسیر پروژه برای import کردن main
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from fastapi.testclient import TestClient
from main import app, vanna_wrapper


client = TestClient(app)


def test_health_endpoint() -> None:
    """تست که endpoint /health پاسخ ۲۰۰ بدهد."""
    resp = client.get("/health")
    assert resp.status_code == 200
    data = resp.json()
    assert "status" in data
    assert "ollama_available" in data
    assert "model" in data


def test_train_endpoint() -> None:
    """تست ثبت شما دیتابیس."""
    resp = client.post("/train", json={"schema": "CREATE TABLE Users (Id INT, Name NVARCHAR(100))"})
    assert resp.status_code == 200
    assert vanna_wrapper.schema_loaded is True


def test_generate_invalid_prompt() -> None:
    """تست اعتبارسنجی پرامپت خالی."""
    resp = client.post("/generate", json={"prompt": ""})
    assert resp.status_code == 422  # validation error


def test_root_endpoint() -> None:
    """تست صفحه اصلی."""
    resp = client.get("/")
    assert resp.status_code == 200
    assert resp.json()["service"] == "Vanna Microservice"


def test_extract_sql_pure() -> None:
    """تست استخراج SQL بدون markdown fences."""
    raw = "SELECT TOP 100 * FROM Customers"
    assert vanna_wrapper._extract_sql(raw) == "SELECT TOP 100 * FROM Customers"


def test_extract_sql_with_fences() -> None:
    """تست استخراج SQL از خروجی markdown."""
    raw = "```sql\nSELECT TOP 100 * FROM Customers\n```"
    assert vanna_wrapper._extract_sql(raw) == "SELECT TOP 100 * FROM Customers"


def test_extract_sql_empty() -> None:
    """تست استخراج SQL از رشته خالی."""
    assert vanna_wrapper._extract_sql("") == ""
    assert vanna_wrapper._extract_sql("   ") == ""
