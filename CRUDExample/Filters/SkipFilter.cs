using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters
{
    public class SkipFilter : Attribute, IFilterMetadata
    {
        public SkipFilter()
        {
        }
    }
}
