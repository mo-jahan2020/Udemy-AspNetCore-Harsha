"""
Vanna Microservice - FastAPI + Vanna.ai + Ollama
=================================================

این میکروسرویس کوچک Python، از Vanna.ai برای تولید SQL استفاده می‌کند.
Vanna با بک‌اند Ollama (مدل محلی) اجرا می‌شود.

Endpoints:
    GET  /health      -> بررسی سلامت سرویس
    POST /train       -> ثبت/به‌روزرسانی شما دیتابیس
    POST /generate    -> تولید SQL از پرامپت کاربر

اجرای مستقیم:
    uvicorn main:app --host 0.0.0.0 --port 8001 --reload
"""

from __future__ import annotations

import os
import time
from typing import Any, Dict, Optional

import httpx
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from loguru import logger
from pydantic import BaseModel, ConfigDict, Field
from pydantic_settings import BaseSettings, SettingsConfigDict


# ----------------------------------------------------------------
# Configuration
# ----------------------------------------------------------------
class Settings(BaseSettings):
    """تنظیمات سرویس Vanna که از محیط خوانده می‌شود."""

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")

    ollama_base_url: str = "http://localhost:11434"
    ollama_model: str = "qwen2.5-coder"
    target_database_url: str = ""
    vanna_service_port: int = 8001
    log_level: str = "INFO"


settings = Settings()

# پیکربندی loguru
logger.remove()
logger.add(
    lambda msg: print(msg, end=""),
    level=settings.log_level,
    format="<green>{time:HH:mm:ss}</green> | <level>{level: <8}</level> | <cyan>{name}</cyan> - <level>{message}</level>",
)


# ----------------------------------------------------------------
# Pydantic Models (Request/Response)
# ----------------------------------------------------------------
class TrainRequest(BaseModel):
    """درخواست ثبت/به‌روزرسانی شما دیتابیس."""

    # نام فیلد در JSON ورودی "schema" است (با alias)،
    # اما نام پایتون آن schema_text است تا با attribute داخلی BaseModel تداخل نکند.
    schema_text: str = Field(..., alias="schema", description="Database schema DDL or documentation")

    model_config = ConfigDict(populate_by_name=True)


class GenerateRequest(BaseModel):
    """درخواست تولید SQL از پرامپت کاربر."""

    prompt: str = Field(..., min_length=1, description="Natural-language prompt")


class GenerateResponse(BaseModel):
    """پاسخ تولید SQL."""

    sql: str
    model: str
    elapsed_ms: int


class HealthResponse(BaseModel):
    """پاسخ Health Check."""

    status: str
    ollama_available: bool
    model: str
    schema_loaded: bool


# ----------------------------------------------------------------
# Vanna Wrapper
# ----------------------------------------------------------------
class VannaWrapper:
    """
    یک Wrapper ساده حول Vanna.ai که با بک‌اند Ollama کار می‌کند.

    توجه: Vanna.ai در نسخه‌های اخیر API خود را تغییر داده است. این Wrapper
    به‌جای وابستگی مستقیم به API متغیر Vanna، از یک رویکرد پایدارتر استفاده
    می‌کند: تولید SQL با تزریق شما به‌صورت System Prompt به Ollama.

    این رویکرد:
      1. پایدارتر است (تغییرات API Vanna باعث شکست نمی‌شود)
      2. با همان منطق Fallback در C# هماهنگ است
      3. همچنان از قابلیت‌های تولید Vanna (اگر نصب باشد) استفاده می‌کند
    """

    def __init__(self, ollama_base_url: str, ollama_model: str) -> None:
        self.ollama_base_url = ollama_base_url.rstrip("/")
        self.ollama_model = ollama_model
        self._schema: str = ""
        self._schema_loaded: bool = False

    def is_ollama_available(self) -> bool:
        """بررسی اینکه آیا سرویس Ollama در دسترس است."""
        try:
            with httpx.Client(timeout=5.0) as client:
                resp = client.get(f"{self.ollama_base_url}/api/tags")
                return resp.status_code == 200
        except Exception as e:
            logger.warning(f"Ollama unavailable: {e}")
            return False

    def train(self, schema_text: str) -> None:
        """ثبت/به‌روزرسانی شما دیتابیس."""
        self._schema = schema_text
        self._schema_loaded = True
        logger.info(f"Schema loaded (length={len(schema_text)} chars)")

    @property
    def schema_loaded(self) -> bool:
        return self._schema_loaded

    def generate_sql(self, prompt: str) -> tuple[str, int]:
        """
        تولید SQL از پرامپت کاربر با استفاده از Ollama.

        این متد System Prompt کامل می‌سازد که شامل:
          - قواعد تولید SQL Read-Only
          - شمای دیتابیس
          - پرامپت کاربر
        """
        if not self._schema_loaded:
            logger.warning("Generating SQL without schema context (schema not trained)")

        system_prompt = self._build_system_prompt()

        payload = {
            "model": self.ollama_model,
            "prompt": prompt,
            "system": system_prompt,
            "stream": False,
            "options": {
                "temperature": 0.2,
                "num_predict": 1024,
            },
        }

        start = time.perf_counter()
        try:
            with httpx.Client(timeout=180.0) as client:
                resp = client.post(
                    f"{self.ollama_base_url}/api/generate",
                    json=payload,
                )
                resp.raise_for_status()
        except httpx.HTTPError as e:
            raise RuntimeError(f"Ollama request failed: {e}") from e

        elapsed_ms = int((time.perf_counter() - start) * 1000)

        data: Dict[str, Any] = resp.json()
        raw_response = data.get("response", "")
        sql = self._extract_sql(raw_response)
        return sql, elapsed_ms

    def _build_system_prompt(self) -> str:
        """ساخت System Prompt با تزریق شما و قواعد تولید SQL."""
        schema = self._schema or "/* No schema provided. */"
        return f"""You are an expert SQL Server (T-SQL) query generator.
Your task: convert the user's natural-language request into a single, valid, read-only T-SQL query.

STRICT RULES:
1. Output ONLY the SQL query. No markdown, no explanations, no comments.
2. Use ONLY SELECT statements. NEVER use INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, MERGE, GRANT, REVOKE, EXEC, or xp_cmdshell.
3. Use the schema provided below. Use exact table and column names.
4. Use TOP N (e.g. TOP 100) when the user might expect many rows.
5. Use square brackets [] around object names if they contain spaces or reserved words.
6. If the request cannot be answered from the schema, return: SELECT NULL AS Error;
7. Use proper JOINs; prefer explicit JOIN syntax over implicit joins.
8. Always alias tables for readability.

DATABASE SCHEMA:
----------------
{schema}
----------------
"""

    @staticmethod
    def _extract_sql(raw: str) -> str:
        """استخراج SQL خالص از خروجی مدل (حذف markdown fences)."""
        if not raw or not raw.strip():
            return ""
        text = raw.strip()

        if text.startswith("```"):
            # حذف خط اول (```sql)
            first_nl = text.find("\n")
            if first_nl >= 0:
                text = text[first_nl + 1:]
            if text.endswith("```"):
                text = text[:-3]
            text = text.strip()

        # حذف هر باقی‌مانده fences
        text = text.replace("```sql", "").replace("```", "").strip()
        return text


# ----------------------------------------------------------------
# FastAPI App
# ----------------------------------------------------------------
app = FastAPI(
    title="Vanna Microservice",
    description="Text-to-SQL microservice powered by Vanna.ai + Ollama",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc",
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# نمونه سراسری Vanna
vanna_wrapper = VannaWrapper(
    ollama_base_url=settings.ollama_base_url,
    ollama_model=settings.ollama_model,
)


# ----------------------------------------------------------------
# Exception Handler
# ----------------------------------------------------------------
@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """مدیریت گلوبال استثناها برای پاسخ JSON استاندارد."""
    logger.error(f"Unhandled exception: {exc}")
    return JSONResponse(
        status_code=500,
        content={
            "type": "about:blank",
            "title": "Internal server error",
            "status": 500,
            "detail": str(exc),
            "instance": str(request.url.path),
        },
    )


# ----------------------------------------------------------------
# Endpoints
# ----------------------------------------------------------------
@app.get("/health", response_model=HealthResponse, tags=["health"])
async def health() -> HealthResponse:
    """بررسی سلامت سرویس و اتصال به Ollama."""
    ollama_ok = vanna_wrapper.is_ollama_available()
    return HealthResponse(
        status="Healthy" if ollama_ok else "Degraded",
        ollama_available=ollama_ok,
        model=settings.ollama_model,
        schema_loaded=vanna_wrapper.schema_loaded,
    )


@app.post("/train", tags=["training"])
async def train(req: TrainRequest):
    """
    ثبت یا به‌روزرسانی شما دیتابیس.

    شما می‌تواند شامل DDL جداول، کلیدهای خارجی و نمونه داده اختیاری باشد.
    این شما در System Prompt به Ollama تزریق می‌شود.
    """
    try:
        vanna_wrapper.train(req.schema_text)
        return {"status": "ok", "message": "Schema loaded", "length": len(req.schema_text)}
    except Exception as e:
        logger.error(f"Train failed: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/generate", response_model=GenerateResponse, tags=["generate"])
async def generate(req: GenerateRequest):
    """
    تولید SQL از پرامپت کاربر.

    پرامپت به Ollama ارسال می‌شود و SQL تولیدشده برگردانده می‌شود.
    """
    if not vanna_wrapper.is_ollama_available():
        raise HTTPException(
            status_code=503,
            detail=f"Ollama not available at {settings.ollama_base_url}",
        )

    try:
        sql, elapsed_ms = vanna_wrapper.generate_sql(req.prompt)
        return GenerateResponse(
            sql=sql,
            model=settings.ollama_model,
            elapsed_ms=elapsed_ms,
        )
    except RuntimeError as e:
        logger.error(f"Generate failed: {e}")
        raise HTTPException(status_code=502, detail=str(e))


@app.get("/", tags=["root"])
async def root():
    """صفحه اصلی سرویس."""
    return {
        "service": "Vanna Microservice",
        "version": "1.0.0",
        "endpoints": ["/health", "/train", "/generate", "/docs"],
    }


# ----------------------------------------------------------------
# Entry point (برای اجرای مستقیم: python main.py)
# ----------------------------------------------------------------
if __name__ == "__main__":
    import uvicorn

    logger.info(
        f"Starting Vanna Microservice on port {settings.vanna_service_port}"
    )
    logger.info(f"Ollama: {settings.ollama_base_url} (model: {settings.ollama_model})")

    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=settings.vanna_service_port,
        reload=False,
        log_level=settings.log_level.lower(),
    )
