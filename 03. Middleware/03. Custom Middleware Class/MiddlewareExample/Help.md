Request: GET /
   │
   ↓ (Pipeline شروع میشه)
   
[Middleware 1: app.Use]                    → می‌نویسه: "From Midleware 1"
   │ (await next(context))
   ↓
[Middleware 2: MyCustomMiddleware]         → می‌نویسه: "My Custom Middleware - Starts"
   │ (await next(context))
   ↓
[Middleware 3: app.Run - Terminal]         → می‌نویسه: "From Middleware 3"
   │ (next صدا زده نمیشه - پایان!)
   ↑
[Middleware 2: برگشت از next]             → می‌نویسه: "My Custom Middleware - Ends"
   ↑
[Middleware 1: برگشت از next]             → (کدی نیست)

📤 خروجی نهایی:
┌────────────────────────────────────────┐
│ From Midleware 1                        │
│ My Custom Middleware - Starts           │
│ From Middleware 3                       │
│ My Custom Middleware - Ends             │
└────────────────────────────────────────┘