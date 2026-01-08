using Microsoft.AspNetCore.Mvc.Rendering;

namespace HNTAS.Web.UI.Helpers
{
    public static class OrganisationHelper
    {
        public static List<SelectListItem> GetOrganisationTypeOptions()
        {
            return
            [
                new SelectListItem { Value = Models.Enums.OrganisationType.UkCompaniesHouse.ToString(), Text = "UK company registered with Companies House" },
                new SelectListItem { Value = Models.Enums.OrganisationType.OtherUkOrganisation.ToString(), Text = "Other UK organisation" },
                new SelectListItem { Value = Models.Enums.OrganisationType.OverseasOrganisation.ToString(), Text = "Overseas organisation" }
            ];
        }
    }
}
