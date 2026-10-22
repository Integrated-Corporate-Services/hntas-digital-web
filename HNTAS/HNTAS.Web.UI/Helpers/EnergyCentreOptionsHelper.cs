using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models;

namespace HNTAS.Web.UI.Helpers
{
    public static class EnergyCentreOptionsHelper
    {
        public static List<Option> GetOptions(string elementId) => new()
        {
                new()
                {
                    OptId = elementId,
                    OptValue = nameof(NullableOfEnergyCentreType.SuppliesMultipleBuildings),
                    OptText = "Yes, it supplies multiple buildings"
                },
                new()
                {
                    OptId = $"{elementId}-2",
                    OptValue = nameof(NullableOfEnergyCentreType.SeparateDistrictNetworkMultipleBuildings),
                    OptText = "No, multiple buildings are supplied by a separate district network"
                },
                new()
                {
                    OptId = $"{elementId}-3",
                    OptValue = nameof(NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly),
                    OptText = "No, the energy centre supplies one communal building only"
                },
                new()
                {
                    OptId = $"{elementId}-4",
                    OptValue = nameof(NullableOfEnergyCentreType.SingleCommunalBuildingSeparateDistrictNetwork),
                    OptText = "No, a single communal building is supplied by a separate district network"
                }
        };
    }
}
