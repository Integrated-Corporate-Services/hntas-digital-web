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
                new() { Id = NetworkDetailsType.Soa, Label = "Statement of Applicability", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
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

        public static ElementTypeInShort GetNetworkElementIdByType(string elementType)
        {
            return elementType switch
            {
                "EnergyCentre" => ElementTypeInShort.EC,
                "Substation" => ElementTypeInShort.SS,
                "DistrictDistribution" => ElementTypeInShort.DDN,
                "ConsumerConnection" => ElementTypeInShort.CC,
                "CommunalDistribution" => ElementTypeInShort.CDN,                
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element type value: {elementType}")
            };
        }

        public static HeatNetworkElementType GetNetworkElementDisplayTypeById(ElementTypeInShort? elementId)
        {
            return elementId switch
            {
                ElementTypeInShort.EC => HeatNetworkElementType.EnergyCentre,
                ElementTypeInShort.SS => HeatNetworkElementType.Substation,
                ElementTypeInShort.DDN => HeatNetworkElementType.DistrictDistribution,
                ElementTypeInShort.CC => HeatNetworkElementType.ConsumerConnection,
                ElementTypeInShort.CDN => HeatNetworkElementType.CommunalDistribution,
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }

        public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(Api.Client.Model.HeatNetworkType? networkType = null, bool hasOwnEnergyCentre = false)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.Communal)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.Substation, Label = "Communal substation (within the communal building)", SubLabel = "Substations", Hint = "Helps supply a communal distribution network" },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal Distribution Network", SubLabel = "Communal Distribution Networks", Hint = "Pipework running inside a communal building to dwellings or units"},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "Connects the network to individual dwellings or units" },
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.District && hasOwnEnergyCentre)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre (excluding the main energy centre)", SubLabel = "Energy Centres", Hint = "The plant room containing heat generation and connection equipment to an energy source" },
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer Connection", SubLabel = "Consumer Connections", Hint = "The connection between a district or communal distribution network and a single consumer heat system"},
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.District && !hasOwnEnergyCentre)
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
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy Centre", Hint = " (not including the supplying energy centre)" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District Distribution Network"},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substation"},
                };
            }
        }

        public static string GetNetworkElementHeadingForNetworkType(Api.Client.Model.HeatNetworkType? networkType, bool hasOwnEc)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.Communal)
            {
                return "Communal network elements";
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.District)
            {
                return "District network elements";
            }

            return "";
        }

        public static string GetNetworkTypeLabelForNetworkType(Api.Client.Model.HeatNetworkType? networkType, bool hasOwnEc)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.Communal && hasOwnEc)
                return HeatNetworkTypeConstants.CommunalWithIntegralEC;
            if (networkType == Api.Client.Model.HeatNetworkType.Communal && !hasOwnEc)
                return HeatNetworkTypeConstants.CommunalWithSeparateUpstreamHN;
            if (networkType == Api.Client.Model.HeatNetworkType.District && hasOwnEc)
                return HeatNetworkTypeConstants.DistrictWithOwnEC;
            if (networkType == Api.Client.Model.HeatNetworkType.District && !hasOwnEc)
                return HeatNetworkTypeConstants.DistrictWithSeparateUpstreamHN;

            throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}");
        }

        public static string GetNetworkTypeLabel(Api.Client.Model.HeatNetworkType? networkType)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.Communal)
                return HeatNetworkTypeConstants.Communal;
            if (networkType == Api.Client.Model.HeatNetworkType.District)
                return HeatNetworkTypeConstants.District;            

            throw new ArgumentOutOfRangeException(nameof(networkType), $"Not expected heat network type value: {networkType}");
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

        public static string GetNetworkElementLabelByElementId(ElementTypeInShort? elementId)
        {
            return elementId switch
            {
                ElementTypeInShort.EC => "Energy centre",
                ElementTypeInShort.SS => "Substation",
                ElementTypeInShort.DDN => "District distribution network",
                ElementTypeInShort.CC => "Consumer connections",
                ElementTypeInShort.CDN => "Communal distribution network",
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }


        // new changes

        public static List<ExistingNetworkElementOption> GetExistingNetworkElementOptionsForNetworkType(Api.Client.Model.HeatNetworkType? networkType = null, bool hasOwnEnergyCentre = false)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.District)
            {
                return new List<ExistingNetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District distribution networks", Hint = "The district distribution network connects the energy centre to the buildings it serves."},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "The substation is the point where the heat network connects to the building. It contains heat exchangers and metering equipment." },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal distribution networks", Hint = "The communal distribution network supplies dwellings or units within one communal building."},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections", Hint = "The connection between a communal distribution network and a single consumer heat system" },
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.Communal)
            {
                return new List<ExistingNetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District distribution networks", Hint = "The district distribution network connects the energy centre to the buildings it serves."},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "The substation is the point where the heat network connects to the building. It contains heat exchangers and metering equipment." },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal distribution networks", Hint = "The communal distribution network supplies dwellings or units within one communal building."},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections", Hint = "The connection between a communal distribution network and a single consumer heat system" },
                };
            }            
            else
            {
                return new List<ExistingNetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District distribution networks", Hint = "The district distribution network connects the energy centre to the buildings it serves."},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "The substation is the point where the heat network connects to the building. It contains heat exchangers and metering equipment." },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal distribution networks", Hint = "The communal distribution network supplies dwellings or units within one communal building."},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections", Hint = "The connection between a communal distribution network and a single consumer heat system" },
                };
            }
        }

        public static ExistingNetworkEcNameViewModel GetExistingNetworkEcName(ExistingNetworkElementOption elementOption)
        {
            var existingEcNameModel = new ExistingNetworkEcNameViewModel();
            for(int i = 0; i < elementOption.ExistingCount; i++)
            {                
                existingEcNameModel.ExistingEcNameOptions.Add(new EcNameOption { Label = $"{i + 1}. Existing energy centre" });
            }

            for(int i = 0; i < elementOption.NewCount; i++)
            {
                if (i == 0)
                    existingEcNameModel.NewEcNameOptions.Add(new EcNameOption { Label = $"{i + 1}. Main energy centre", IsMainEc = true });
                else
                    existingEcNameModel.NewEcNameOptions.Add(new EcNameOption { Label = $"{i + 1}. New energy centre" });
            }

            return existingEcNameModel;
        }
    }
}