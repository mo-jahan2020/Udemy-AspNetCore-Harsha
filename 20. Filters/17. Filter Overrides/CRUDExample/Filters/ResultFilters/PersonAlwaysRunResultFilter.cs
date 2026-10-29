using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonAlwaysRunResultFilter : IAlwaysRunResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            // آیا در زنجیره فیلترهای این درخواست، SkipFilter وجود دارد؟
            if (context.Filters.OfType<SkipFilter>().Any())
            {
                return;// بله → این فیلتر خودش را معاف می‌کند
            }

            //TO DO: before logic here
        }
    }
}
