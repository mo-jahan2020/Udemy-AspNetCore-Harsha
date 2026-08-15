using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModelValidationsExample.CustomValidators
{
    // این کلاس یک Custom Validation Attribute است که برای مقایسه‌ی دو فیلد تاریخ
    // در یک مدل استفاده می‌شود (مثلاً بررسی این‌که تاریخ پایان، از تاریخ شروع بزرگ‌تر باشد).
    // برخلاف اعتبارسنج‌های ساده (مثل Required یا Range) که فقط روی یک Property کار می‌کنند،
    // این Validator باید مقدار Property دیگری از همان کلاس را هم بخواند،
    // به همین دلیل از Reflection استفاده می‌کند.
    public class DateRangeValidatorAttribute : ValidationAttribute
    {
        // نام Property دیگری که باید با آن مقایسه شود مثلاً "FromDate"
        // این مقدار در زمان اعمال Attribute روی Property، به‌صورت پارامتر پاس داده می‌شود.
        // مثال استفاده: [DateRangeValidator("FromDate", ErrorMessage = "...")]
        public string OtherPropertyName { get; set; }

        // سازنده‌ی کلاس - نام Property مقابل را از بیرون (هنگام تعریف روی مدل) دریافت می‌کند
        public DateRangeValidatorAttribute(string otherPropertyName)
        {
            OtherPropertyName = otherPropertyName;
        }

        // این متد قلب اصلی اعتبارسنجی است.
        // - value: مقدار همان Property‌ای است که این Attribute روی آن قرار گرفته (مثلاً ToDate)
        // - validationContext: اطلاعاتی درباره‌ی کل شیء (Object) در حال اعتبارسنجی را در اختیار ما می‌گذارد،
        //   از جمله دسترسی به instance واقعی مدل و سایر Property های آن
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // اگر مقدار فیلد فعلی (مثلاً ToDate) خالی/null نباشد ادامه بده
            if (value != null)
            {
                // مرحله ۱: مقدار فیلد فعلی (که این Attribute روی آن نوشته شده) را به DateTime تبدیل می‌کنیم
                // این همان "تاریخ پایان" (to_date) است
                DateTime to_date = Convert.ToDateTime(value);

                // مرحله ۲: با استفاده از Reflection، اطلاعات Property مقابل (مثلاً FromDate) را
                // از روی نوع (Type) کلاس مدل پیدا می‌کنیم.
                // validationContext.ObjectType نوع کلاسی است که در حال Validate شدن است (مثلاً typeof(Person))
                PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(OtherPropertyName);

                // اگر Property مقابل واقعاً در کلاس وجود داشت (یعنی نام درست وارد شده بود)
                if (otherProperty != null)
                {
                    // مرحله ۳: مقدار واقعی آن Property را از روی خودِ instance فعلی مدل می‌خوانیم
                    // validationContext.ObjectInstance همان object اصلی است (مثلاً همان Person که کاربر پر کرده)
                    // GetValue باعث می‌شود مقدار فعلی آن Property را در زمان اجرا (Runtime) بگیریم
                    DateTime from_date = Convert.ToDateTime(otherProperty.GetValue(validationContext.ObjectInstance));

                    // مرحله ۴: مقایسه‌ی منطقی - اگر تاریخ شروع بزرگ‌تر از تاریخ پایان بود، یعنی خطای منطقی داریم
                    if (from_date > to_date)
                    {
                        // خطا را برمی‌گردانیم.
                        // پارامتر دوم (memberNames) نام فیلدهایی است که این خطا به آن‌ها مرتبط است؛
                        // اینجا هم فیلد مقابل (OtherPropertyName) و هم فیلد فعلی (MemberName) را معرفی می‌کنیم
                        // تا در صورت نیاز، پیام خطا بتواند به هر دو Property نسبت داده شود (مثلاً در Summary یا کنار هر دو Input)
                        return new ValidationResult(ErrorMessage, new string[] { OtherPropertyName, validationContext.MemberName });
                    }
                    else
                    {
                        // در غیر این صورت، اعتبارسنجی موفق بوده است
                        return ValidationResult.Success;
                    }
                }

                // اگر Property مقابل با این نام در کلاس پیدا نشد (مثلاً به‌خاطر تایپی اشتباه در نام)
                // به‌صورت خاموش null برمی‌گردانیم؛
                // نکته: بهتر است اینجا یک Exception پرتاب شود تا خطای برنامه‌نویسی زودتر مشخص شود،
                // چون null برگرداندن یعنی "بدون خطا" تلقی می‌شود و مشکل به‌صورت خاموش نادیده گرفته می‌شود
                return null;
            }

            // اگر مقدار فیلد فعلی از ابتدا null بود، این Validator کاری انجام نمی‌دهد
            // (بررسی خالی نبودن فیلد وظیفه‌ی [Required] است، نه این Validator)
            return null;
        }
    }
}