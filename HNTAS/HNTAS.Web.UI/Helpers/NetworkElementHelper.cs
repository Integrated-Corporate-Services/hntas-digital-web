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

        public static void UpdateOptionStatus<TStatus, TDependentStatus>(NetworkDetailsOption option, TStatus? status, TDependentStatus? dependentStatus = null, bool enabledOnComplete = true)
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
                option.IsEnabled = enabledOnComplete;
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
                "DistrictDistribution" => "DDN",
                "ConsumerConnection" => "CC",
                "CommunalDistribution" => "CDN",                
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element type value: {elementType}")
            };
        }

        public static HeatNetworkElementType GetNetworkElementDisplayTypeById(string elementId)
        {
            return elementId switch
            {
                "EC" => HeatNetworkElementType.EnergyCentre,
                "SS" => HeatNetworkElementType.Substation,
                "DDN" => HeatNetworkElementType.DistrictDistribution,
                "CC" => HeatNetworkElementType.ConsumerConnection,
                "CDN" => HeatNetworkElementType.CommunalDistribution,
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }        

        public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(Api.Client.Model.HeatNetworkType? networkType = null)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.CommunalWithoutEnergyCentre || networkType == Api.Client.Model.HeatNetworkType.CommunalWithEnergyCentre)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.Substation, Label = "Communal substation (within the communal building)", SubLabel = "Substations", Hint = "Helps supply a communal distribution network" },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Networks", Hint = "Pipework running inside a communal building to dwellings or units"},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "Connects the network to individual dwellings or units" },
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.DistrictWithOwnMainEnergyCentre)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre (excluding the main energy centre)", SubLabel = "Energy Centres", Hint = "The plant room containing heat generation and connection equipment to an energy source" },
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "The connection between a district or communal distribution network and a single consumer heat system"},
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.DistrictWithoutOwnMainEnergyCentre)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre (not including the supplying energy centre)", SubLabel = "Energy Centres", Hint = "The plant room containing heat generation and connection equipment to an energy source" },
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "The connection between a district or communal distribution network and a single consumer heat system"},
                };
            }            
            else
            {
                return new List<NetworkElementOption>
                {                    
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network"},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection" },
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District Distribution Network"},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substation"},
                };
            }
        }        

        public static string GetNetworkElementHeadingForNetworkType(Api.Client.Model.HeatNetworkType? networkType)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.CommunalWithoutEnergyCentre || networkType == Api.Client.Model.HeatNetworkType.CommunalWithEnergyCentre)
            {
                return "Communal network elements";
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.DistrictWithoutOwnMainEnergyCentre || networkType == Api.Client.Model.HeatNetworkType.DistrictWithOwnMainEnergyCentre)
            {
                return "District network elements";
            }
            
            return "";
        }

        public static string GetNetworkTypeLabelForNetworkType(Api.Client.Model.HeatNetworkType? networkType)
        {
            return networkType switch
            {
                Api.Client.Model.HeatNetworkType.CommunalWithEnergyCentre => HeatNetworkTypeConstants.CommunalWithIntegralEC,
                Api.Client.Model.HeatNetworkType.CommunalWithoutEnergyCentre => HeatNetworkTypeConstants.CommunalWithSeparateUpstreamHN,
                Api.Client.Model.HeatNetworkType.DistrictWithOwnMainEnergyCentre => HeatNetworkTypeConstants.DistrictWithOwnEC,
                Api.Client.Model.HeatNetworkType.DistrictWithoutOwnMainEnergyCentre => HeatNetworkTypeConstants.DistrictWithSeparateUpstreamHN,
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}")
            };
        }

        public static string GetNetworkTypeLabelForNetworkType(HeatNetworkElementType? networkType)
        {
            return networkType switch
            {
                HeatNetworkElementType.Substation => "Substation",
                HeatNetworkElementType.EnergyCentre => "Energy centre",
                HeatNetworkElementType.ConsumerConnection => "Consumer connections",
                HeatNetworkElementType.DistrictDistribution => "District distribution network",
                HeatNetworkElementType.CommunalDistribution => "Communal distribution network",
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}")
            };
        }
    }
}