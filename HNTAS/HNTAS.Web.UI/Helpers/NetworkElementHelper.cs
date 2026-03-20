using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.NetworkElements;

namespace HNTAS.Web.UI.Helpers
{
    public static class NetworkElementHelper
    {
        public static List<NetworkDetailsOption> GetDefaultNetworkDetailsOptions()
        {
            return new List<NetworkDetailsOption>
            {

                new() { Id = NetworkDetailsType.NetworkElements, Label = "Network elements", Hint = "", UiStatus = StatusConstants.ReadyToStart, IsEnabled = true },
                new() { Id = NetworkDetailsType.Soa, Label = "Element Statement of Applicability", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
            };
        }

        public static void UpdateOptionStatus<TStatus, TDependentStatus>(NetworkDetailsOption option, TStatus? status, TDependentStatus? dependentStatus = null)
    where TStatus : struct, Enum
    where TDependentStatus : struct, Enum
        {
            var statusName = status != null ? Enum.GetName(typeof(TStatus), status.Value) : null;
            var dependentStatusName = dependentStatus != null ? Enum.GetName(typeof(TDependentStatus), dependentStatus!.Value) : null;

            if (dependentStatusName == null && statusName == null)
                return;

            if (dependentStatusName == "Complete" && (statusName == null || statusName == "ReadyToStart"))
            {
                option.UiStatus = StatusConstants.ReadyToStart;
                option.IsEnabled = true;
            }
            else if (statusName == "Complete")
            {
                option.UiStatus = StatusConstants.Completed;
                option.IsEnabled = true;
            }
            else if (statusName == "InProgress")
            {
                option.UiStatus = StatusConstants.InProgress;
                option.IsEnabled = true;
            }
        }

        public static string GetNetworkElementIdByType(string elementType)
        {
            return elementType switch
            {
                "EnergyCentre" => "EC",
                "Substation" => "SS",
                "DistrictDistributionNetwork" => "DDN",
                "ConsumerConnections" => "CC",
                "CommunalDistributionNetwork" => "CDN",
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element type value: {elementType}")
            };

        }

        public static HeatNetworkElementDisplayType GetNetworkElementDisplayTypeById(string elementId)
        {
            return elementId switch
            {
                "EC" => HeatNetworkElementDisplayType.EnergyCentre,
                "SS" => HeatNetworkElementDisplayType.Substation,
                "DDN" => HeatNetworkElementDisplayType.DistrictDistributionNetwork,
                "CC" => HeatNetworkElementDisplayType.ConsumerConnections,
                "CDN" => HeatNetworkElementDisplayType.CommunalDistributionNetwork,
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }

        public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(string networkType = "")
        {
            if (networkType == "CommunalWithIntegralEC")
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementDisplayType.EnergyCentre, Label = "Energy Centre", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment" },
                    new() { Id = HeatNetworkElementDisplayType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
                    new() { Id = HeatNetworkElementDisplayType.CommunalDistributionNetwork, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
                    new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
                };
            }
            else if (networkType == "CommunalWithSeparateUpstreamHN")
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementDisplayType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
                    new() { Id = HeatNetworkElementDisplayType.CommunalDistributionNetwork, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
                    new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
                };
            }
            else if (networkType == "DistrictWithOwnEC")
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementDisplayType.EnergyCentre, Label = "Energy Centre (Not including the main energy centre)", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment, often in a separate building" },
                    new() { Id = HeatNetworkElementDisplayType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
                    new() { Id = HeatNetworkElementDisplayType.DistrictDistributionNetwork, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
                    new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
                };
            }
            else if (networkType == "DistrictWithSeparateUpstreamHN")
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementDisplayType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
                    new() { Id = HeatNetworkElementDisplayType.DistrictDistributionNetwork, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
                    new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
                };
            }
            else
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementDisplayType.EnergyCentre, Label = "Energy Centre", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment" },
                    new() { Id = HeatNetworkElementDisplayType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
                    new() { Id = HeatNetworkElementDisplayType.CommunalDistributionNetwork, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
                    new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
                    new() { Id = HeatNetworkElementDisplayType.DistrictDistributionNetwork, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
                };
            }
        }

        public static string GetNetworkElementHeadingForNetworkType(string networkType)
        {
            return networkType switch
            {
                "CommunalWithIntegralEC" => "Communal network elements",
                "CommunalWithSeparateUpstreamHN" => "Communal network elements",
                "DistrictWithOwnEC" => "District network elements",
                "DistrictWithSeparateUpstreamHN" => "District network elements",
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}")
            };
        }
    }
}
