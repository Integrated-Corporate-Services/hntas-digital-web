using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using HNTAS.Web.UI.CustomValidation;
using HNTAS.Web.UI.Models.Components;
using HNTAS.Api.Client.Model;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.HeatNetworkRegistration
{   

    public class HeatNetworkConnectionCheckboxItem : CheckboxItemWithConditionalInput
    {
        [RequiredIfSelected(ErrorMessage = "Enter the number of connections")]
        [Range(1, 9999, ErrorMessage = "Enter a value between 1 and 9999")]
        public new int? ConditionalValue { get; set; }
    }

    public class HeatNetworkConnectionsViewModel
    {
        [MustHaveOneHnConnectionAttribute(ErrorMessage = "Select at least one connection type")]
        public List<HeatNetworkConnectionCheckboxItem> Connections { get; set; } = new();

        public List<string> ConnectionsToDisplay
        {
            get
            {
                var displayList = new List<string>();                
                foreach (var connection in Connections)
                {
                    if (connection.IsSelected)
                    {
                        int count = connection.ConditionalValue.HasValue ? connection.ConditionalValue.Value : 0;
                        if (connection.Value == ConnectionType.CommunalBuildings.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add(FormatCount(count, "communal building"));
                        }
                        else if (connection.Value == ConnectionType.IndividualHomes.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add(FormatCount(count, "domestic consumer"));
                        }
                        else if (connection.Value == ConnectionType.CommercialConnection.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add(FormatCount(count, "non-domestic consumer"));
                        }
                        else if (connection.Value == ConnectionType.OtherDistrictNetwork.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add(FormatCount(count, "district connection"));
                        }
                    }
                }
                return displayList;
            }
        }

        private static string FormatCount(int count, string label)
        {
            return $"{count} {label}{(count == 1 ? string.Empty : "s")}";
        }
    }
}
