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

        public static string GetNetworkElementLabelByElementType(HeatNetworkElementType elementType)
        {
            return elementType switch
            {
                HeatNetworkElementType.EnergyCentre => "Energy Centre",
                HeatNetworkElementType.Substation => "Substation",
                HeatNetworkElementType.DistrictDistribution => "District Distribution Network",
                HeatNetworkElementType.ConsumerConnection => "Consumer Connection",
                HeatNetworkElementType.CommunalDistribution => "Communal Distribution Network",
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element ID value: {elementType}")
            };
        }

        //public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(Api.Client.Model.HeatNetworkType? networkType = null)
        //{
        //    if (networkType == Api.Client.Model.HeatNetworkType.CommunalWithIntegralEC)
        //    {
        //        return new List<NetworkElementOption>
        //        {
        //            new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment" },
        //            new() { Id = HeatNetworkElementType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
        //            new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
        //            new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
        //        };
        //    }
        //    else if (networkType == Api.Client.Model.HeatNetworkType.CommunalWithSeparateUpstreamHN)
        //    {
        //        return new List<NetworkElementOption>
        //        {
        //            new() { Id = HeatNetworkElementType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
        //            new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
        //            new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
        //        };
        //    }
        //    else if (networkType == Api.Client.Model.HeatNetworkType.DistrictWithOwnEC)
        //    {
        //        return new List<NetworkElementOption>
        //        {
        //            new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre (Not including the main energy centre)", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment, often in a separate building" },
        //            new() { Id = HeatNetworkElementType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
        //            new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
        //            new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
        //        };
        //    }
        //    else if (networkType == Api.Client.Model.HeatNetworkType.DistrictWithSeparateUpstreamHN)
        //    {
        //        return new List<NetworkElementOption>
        //        {
        //            new() { Id = HeatNetworkElementType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
        //            new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
        //            new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
        //        };
        //    }
        //    else
        //    {
        //        return new List<NetworkElementOption>
        //        {
        //            new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre", SubLabel = "Energy Centre", Hint = "for example, a plant room with heat generation equipment" },
        //            new() { Id = HeatNetworkElementType.Substation, Label = "Substation", SubLabel = "Substation", Hint = "for example, a heat exchanger connecting a building" },
        //            new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Network", Hint = "for example, pipework running inside a communal block to dwellings"},
        //            new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connection", Hint = "for example, a heat interface unit (HIU) connecting a dwelling" },
        //            new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District Distribution Network", SubLabel = "District Distribution Network", Hint = "for example, pipework (often underground) running from the energy centre to buildings"},
        //        };
        //    }
        //}

        public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(string networkType, bool isOwnEnergyCentre)
        {
            if (networkType == "Communal")
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Communal substation (within the communal building)", SubLabel = "Substations", Hint = "Helps supply a communal distribution network" },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Networks", Hint = "Pipework running inside a communal building to dwellings or units"},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "Connects the network to individual dwellings or units" },
                };
            }
            else if (networkType == "District" && isOwnEnergyCentre)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre (excluding the main energy centre)", SubLabel = "Energy Centres", Hint = "The plant room containing heat generation and connection equipment to an energy source" },
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "The connection between a district or communal distribution network and a single consumer heat system"},
                };
            }
            else if (networkType == "District" && !isOwnEnergyCentre)
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
                };
            }
        }

        //public static string GetNetworkElementHeadingForNetworkType(Api.Client.Model.HeatNetworkType? networkType)
        //{
        //    return networkType switch
        //    {
        //        Api.Client.Model.HeatNetworkType.CommunalWithIntegralEC => "Communal network elements",
        //        Api.Client.Model.HeatNetworkType.CommunalWithSeparateUpstreamHN => "Communal network elements",
        //        Api.Client.Model.HeatNetworkType.DistrictWithOwnEC => "District network elements",
        //        Api.Client.Model.HeatNetworkType.DistrictWithSeparateUpstreamHN => "District network elements",
        //        _ => throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}")
        //    };
        //}

        public static string GetNetworkElementHeadingForNetworkType(string networkType)
        {
            if (networkType == "Communal")
            {
                return "Communal network elements";
            }
            else if (networkType == "District")
            {
                return "District network elements";
            }
            
            return "";
        }

        public static string GetNetworkTypeLabelForNetworkType(Api.Client.Model.HeatNetworkType? networkType)
        {
            return networkType switch
            {
                Api.Client.Model.HeatNetworkType.CommunalWithIntegralEC => HeatNetworkTypeConstants.CommunalWithIntegralEC,
                Api.Client.Model.HeatNetworkType.CommunalWithSeparateUpstreamHN => HeatNetworkTypeConstants.CommunalWithSeparateUpstreamHN,
                Api.Client.Model.HeatNetworkType.DistrictWithOwnEC => HeatNetworkTypeConstants.DistrictWithOwnEC,
                Api.Client.Model.HeatNetworkType.DistrictWithSeparateUpstreamHN => HeatNetworkTypeConstants.DistrictWithSeparateUpstreamHN,
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}")
            };
        }
    }
}