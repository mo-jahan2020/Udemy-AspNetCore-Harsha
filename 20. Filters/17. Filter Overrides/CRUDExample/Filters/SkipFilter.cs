using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters
{
    public class SkipFilter : Attribute, IFilterMetadata
    {
        // یک Attribute خالی که فقط «علامت» است — هیچ منطقی ندارد
    }
}
