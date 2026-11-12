using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.NetworkElements;

namespace HNTAS.Web.UI.Helpers
{
    public static class NetworkElementHelper
    {
        public static List<NetworkDetailsOption> GetDefaultNetworkDetailsOptions(bool ecPlantData)
        {
            if (ecPlantData)
            {
                return new List<NetworkDetailsOption>
                {

                    new() { Id = NetworkDetailsType.NetworkElements, Label = "Network elements", Hint = "", UiStatus = StatusConstants.ReadyToStart, IsEnabled = true },
                    new() { Id = NetworkDetailsType.EnergyCentrePlantData, Label = "Energy centre plant data", Hint = "", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                    new() { Id = NetworkDetailsType.Soa, Label = "Statement of Applicability", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                };
            }
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
                "NonDomesticConsumerConnection" => ElementTypeInShort.NDCC,
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element type value: {elementType}")
            };
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
                HeatNetworkElementType.NonDomesticConsumerConnection => "Non-domestic consumer connections",
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
                ElementTypeInShort.NDCC => "Non-domestic consumer connections",
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }

        public static HeatNetworkElementType GetNetworkElementDisplayTypeByElementId(ElementTypeInShort? elementId)
        {
            return elementId switch
            {
                ElementTypeInShort.EC => HeatNetworkElementType.EnergyCentre,
                ElementTypeInShort.SS => HeatNetworkElementType.Substation,
                ElementTypeInShort.DDN => HeatNetworkElementType.DistrictDistribution,
                ElementTypeInShort.CC => HeatNetworkElementType.ConsumerConnection,
                ElementTypeInShort.CDN => HeatNetworkElementType.CommunalDistribution,
                ElementTypeInShort.NDCC => HeatNetworkElementType.NonDomesticConsumerConnection,
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }       

        public static List<NetworkElementOption> GetNetworkElementOptionsForNetworkType(Api.Client.Model.HeatNetworkType? networkType = null, bool hasOwnEnergyCentre = false)
        {
            if (networkType == Api.Client.Model.HeatNetworkType.District)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District distribution networks", Hint = "The district distribution network connects the energy centre to the buildings it serves."},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "The substation is the point where the heat network connects to the building. It contains heat exchangers and metering equipment." },
                    new() { Id = HeatNetworkElementType.NonDomesticConsumerConnection, Label = "Non-domestic consumer connections", Hint = "The connection between a district network and a single consumer heat system"},
                };
            }
            else if (networkType == Api.Client.Model.HeatNetworkType.Communal)
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "For example, a communal distribution network pressure break" },
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal distribution networks", Hint = "The communal distribution network supplies dwellings or units within one communal building."},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections", Hint = "The connection between a communal distribution network and a single consumer heat system" },
                };
            }            
            else
            {
                return new List<NetworkElementOption>
                {
                    new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centres", Hint = "The energy centre is a plant room that contains heat generation equipment and/or equipment connecting to an energy source. It may be in the building or located nearby" },
                    new() { Id = HeatNetworkElementType.DistrictDistribution, Label = "District distribution networks", Hint = "The district distribution network connects the energy centre to the buildings it serves."},
                    new() { Id = HeatNetworkElementType.Substation, Label = "Substations", Hint = "For example, a communal distribution network pressure break" },
                    new() { Id = HeatNetworkElementType.NonDomesticConsumerConnection, Label = "Non-domestic consumer connections", Hint = "The connection between a district network and a single consumer heat system"},
                    new() { Id = HeatNetworkElementType.CommunalDistribution, Label = "Communal distribution networks", Hint = "The communal distribution network supplies dwellings or units within one communal building."},
                    new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections", Hint = "The connection between a communal distribution network and a single consumer heat system" },
                };
            }
        }

        public static NetworkEcNameViewModel GetNetworkEcName(NetworkElementOption elementOption)
        {
            var existingEcNameModel = new NetworkEcNameViewModel();
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