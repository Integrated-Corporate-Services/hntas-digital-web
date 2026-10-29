using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HNTAS.Web.UI.Extensions
{
    public static class ModelStateExtensions
    {
        public static IEnumerable<(string Key, ModelError Error)> GetOrderedErrors(
    this ModelStateDictionary modelState,
    Type? viewModelType)
        {
            if (viewModelType == null)
            {
                return modelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(error => (kvp.Key, error)));
            }

            var propertyOrder = viewModelType.GetProperties()
                .Select(p => p.Name)
                .ToList();

            return modelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .OrderBy(kvp =>
                {
                    var index = propertyOrder.IndexOf(kvp.Key);
                    return index == -1 ? int.MaxValue : index;
                })
                .SelectMany(kvp => kvp.Value!.Errors.Select(error => (kvp.Key, error)));
        }
    }
}