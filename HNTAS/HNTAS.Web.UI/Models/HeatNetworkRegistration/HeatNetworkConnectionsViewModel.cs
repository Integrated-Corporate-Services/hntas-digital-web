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
                        if (connection.Value == ConnectionType.CommunalBuildings.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add($" {connection.ConditionalValue.Value} communal buildings");
                        }
                        else if (connection.Value == ConnectionType.IndividualHomes.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add($" {connection.ConditionalValue.Value} domestic consumers");
                        }
                        else if (connection.Value == ConnectionType.CommercialConnection.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add($" {connection.ConditionalValue.Value} non domestic consumers");
                        }
                        else if (connection.Value == ConnectionType.OtherDistrictNetwork.ToString() && connection.ConditionalValue.HasValue)
                        {
                            displayList.Add($" {connection.ConditionalValue.Value} district connections");
                        }
                    }
                }
                return displayList;
            }
        }
    }
}
