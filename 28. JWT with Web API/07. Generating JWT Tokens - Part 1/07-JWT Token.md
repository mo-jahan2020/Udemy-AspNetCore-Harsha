
# instal  this packages: 
	1- Core app: Microsoft.AspNetCore.Authentication.JwtBearer
# ۱. توکن JWT چیست؟
	JWT (JSON Web Token) یک استاندارد باز (RFC 7519) برای انتقال امن اطلاعات بین طرفین به صورت یک شیء
	JSON است. این توکن خودکفا (Self-contained) است؛ یعنی تمام اطلاعات لازم (درباره کاربر و مجوزهایش) درون خود توکن قرار دارد 
	و سرور برای احراز هویت نیازی به ذخیره‌سازی session یا کوئری مکرر به پایگاه داده ندارد .

# ساختار JWT
	هر JWT از سه بخش تشکیل شده که با نقطه نقطه از هم جدا می‌شوند:

	1- Header (سربرگ): شامل نوع توکن (typ: JWT) و الگوریتم امضا (مثلاً HS256 یا RS256).
		{
			"type" : "JWT",
			"alg" : "HS256"
		}

	2- Payload بار مفید: شامل Claims ادعاها است. Claim‌ها اطلاعاتی درباره موجودیت (معمولاً کاربر) و متادیتای توکن هستند.
		{
			"userId" : "asaasert-sdfg-dfgh-erty-qwertyuiopas"
		}
		مهم‌ترین Claim‌های استاندارد عبارتند از:

		iss (Issuer): صادرکننده توکن

		sub (Subject): موضوع توکن (معمولاً شناسه کاربر)

		aud (Audience): دریافت‌کننده مورد نظر توکن

		exp (Expiration): زمان انقضا

		iat (Issued At): زمان صدور

		jti (JWT ID): شناسه یکتای توکن 

	3- Signature امضا : برای تأیید اصالت توکن و اطمینان از دستکاری نشدن آن.
		امضا با ترکیب Header و Payload و یک کلید مخفی (Secret) یا کلید خصوصی (Private Key) تولید می‌شود .

	algurithm: data=base64Encoding(headr) + "." + base64Encoding(payload)
			   hashedData= hash(data,secret)
			   signature = base64Encoding(hashedData)
			   jsonToken= data + ". " signature
# ۲. JWT چگونه کار می‌کند؟

	فرایند کلی در یک Web API به شرح زیر است:

		1- احراز هویت (Authentication): کلاینت (مثلاً یک اپلیکیشن موبایل یا SPA) credentials (نام کاربری/رمز عبور) را به یک endpoint ورود در API ارسال می‌کند.

		2- صدور توکن (Token Issuance): سرور پس از تأیید صحت اطلاعات، یک JWT امضا شده تولید کرده و به کلاینت بازمی‌گرداند.

		3- ذخیره‌سازی توکن : کلاینت توکن را در حافظه (مثلاً sessionStorage یا localStorage) ذخیره می‌کند.

		4- ارسال توکن در درخواست‌های بعدی: کلاینت توکن را در هدر Authorization با طرح Bearer قرار می‌دهد

			Authorization: Bearer <token>
		5- اعتبارسنجی (Validation): API در هر درخواست، توکن دریافتی را بررسی می‌کند:

			آیا امضای توکن معتبر است؟ (آیا با کلید صحیح امضا شده؟)

			آیا توکن منقضی نشده است؟ (exp)
		
			آیا صادرکننده (iss) و دریافت‌کننده (aud) مورد انتظار هستند؟ 

	اگر توکن معتبر باشد، API به منابع محافظت‌شده دسترسی می‌دهد؛ در غیر این صورت، پاسخ 401 Unauthorized بازمی‌گرداند.