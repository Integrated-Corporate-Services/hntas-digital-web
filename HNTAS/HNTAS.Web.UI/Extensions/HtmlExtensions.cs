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

        public static bool HasError(this IHtmlHelper htmlHelper, string key)
        {
            // Try to get the ModelState entry for the given key.
            htmlHelper.ViewData.ModelState.TryGetValue(key, out var entry);

            // Return true if the entry exists and has errors.
            return entry?.Errors.Count > 0;
        }
    }
}
