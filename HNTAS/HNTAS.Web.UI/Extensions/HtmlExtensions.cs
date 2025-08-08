using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace HNTAS.Web.UI.Extensions
{
    public static class HtmlExtensions
    {
        public static string LowercaseIdFor<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            var id = htmlHelper.IdFor(expression);
            return id.ToLowerInvariant();
        }
    }
}
