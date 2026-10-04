using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.CustomModelBinders
{
    // IModelBinderProvider : یعنی نگارش خاصی را میخواهیم بایند کنیم
    public class PersonBinderProvider : IModelBinderProvider
 {
        //GetBinder : این متد اجباری است
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
            // اگر مدل کانتکس از نوع پرسون باشد
   if (context.Metadata.ModelType == typeof(Person))
   {
                // بایندر کاستوم شده پرسون را برگردان
    return new BinderTypeModelBinder(typeof(PersonModelBinder));
   }

   return null;
  }
 }
}

