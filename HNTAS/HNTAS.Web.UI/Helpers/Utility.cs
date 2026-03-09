using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.NetworkCharacteristics;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using ApiHeatNetworkType = HNTAS.Api.Client.Model.HeatNetworkType;
using System.Text.Json;

namespace HNTAS.Web.UI.Helpers
{
    public static class Utility
    {
        public static void ShowBackButton(this Controller controller, string action, string controllerName)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName);
        }
        public static void ShowBackButton(this Controller controller, string action)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action);
        }
        public static void ShowBackButton(this Controller controller, string action, string controllerName, object routeValues = null)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, routeValues);
        }

        public static void ShowBackButton(this Controller controller, string action, string controllerName, string fragement)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, "", "", "", fragement);
        }

        public static string CapitalizeCommaSeparated(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.Split(',')
                             .Select(w => w.Trim())
                             .Where(w => !string.IsNullOrEmpty(w))
                             .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower());

            return string.Join(", ", words);

        }

        public static List<HeatNetworkElementOption> GetElementOptions()
        {
            return new List<HeatNetworkElementOption>
            {
                new() { Id = HeatNetworkElementDisplayType.EnergyCentre, Label = "Energy centre", Hint = "Only 1 allowed per heat network unless part of a closed loop." },
                new() { Id = HeatNetworkElementDisplayType.DistributionNetwork, Label = "Distribution network", Hint = "Only 1 allowed per heat network." },
                new() { Id = HeatNetworkElementDisplayType.ThermalSubStation, Label = "Thermal sub station" },
                new() { Id = HeatNetworkElementDisplayType.CommunalDistributionNetwork, Label = "Communal distribution network"},
                new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer connections" },
                new() { Id = HeatNetworkElementDisplayType.ConsumerHeatSystems, Label = "Consumer heat systems" }
            };
        }

        public static List<SelectItemOption> GetContributorSelectList(string userRole)
        {
            if (userRole == UserRole.ResponsiblePerson.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedDesigner).ToString(), Text = "Designated designer" },
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedContractor).ToString(), Text = "Designated contractor" },
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedOperator).ToString(), Text = "Designated operator" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.DesignatedDesigner.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingDesigner).ToString(), Text = "Contributing designer" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.DesignatedContractor.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingContractor).ToString(), Text = "Contributing contractor" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.DesignatedOperator.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingOperator).ToString(), Text = "Contributing operator" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.Assessor.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else
            {
                return null;
            }
        }

        public static async Task<string> GetUserRoleByUserHNMapping(UserResponse user, string hnId)
        {
            var userRole = "";
            if (user?.Roles?.Contains(UserRole.ResponsiblePerson) == true)
            {
                userRole = UserRole.ResponsiblePerson.ToString();
            }
            else
            {
                foreach (var mapping in user.HnRoleMappings)
                {
                    if (mapping.HnId == hnId)
                    {
                        userRole = mapping.Role.ToString();
                    }
                }
            }
            return userRole;
        }

        public static async Task<List<SelectItemOption>?> GetHeatNetworkSelectListAsync(List<HeatNetworkUserResponse> response)
        {
            //var response = await _userService.GetUserHeatNetworks(userId);
            if (response == null) return null;

            return response.Select(hn => new SelectItemOption
            {
                Value = hn.HnId,
                Text = $"{hn.HnId} - {hn.Name}"
            }).ToList();
        }

        public static List<HeatNetworkTypeOption> GetHeatNetworkTypeOptions()
        {

            var heatNetworkOptions = new List<HeatNetworkTypeOption>
            {
                new() {
                    Id = "SelectedHeatNetworkType",
                    Value = ApiHeatNetworkType.NetworkLedDistrictHeatNetwork.ToString(),
                    Text = "Network‑led District Heat Network",
                    Hint = "An Energy Centre or District Distribution Network which is being developed as part of a network-led District Heating Network",
                    SummaryText = "Help with network-led District Heat Networks",
                    DetailsText = "A network-led District Heating Network is a heat network which supplies two or more buildings, and that is developed independently of the boundaries of any particular development, with third parties connecting to that heat network.\r\n\r\nThis would cover both District Heating Networks which are connecting to existing buildings and/or pre-existing heat networks (such as city-wide district heating networks constructed with HNIP or GHNF funding connecting to public buildings, campus networks, etc.), and those which serve new building developments developed by plot developers (but developed and constructed independently to the new buildings)."
                },
                new() {
                    Id = "SelectedHeatNetworkType-2",
                    Value = ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkMorL.ToString(),
                    Text = "Developer‑led District Heat Network(medium‑large)",
                    Hint = "An Energy Centre or District Distribution Network which is being developed as part of a new build medium-large developer-led District Heating Network",
                    SummaryText = "Help with developer-led District Heat Networks",
                    DetailsText = "A developer-led District Heating Network refers to a heat network that is built to service a single development, which contains two or more buildings. Normally the heat network would be constructed simultaneously with the wider building works, but this can also include heat networks retrofitted to a single building or estate."
                },
                new() {
                    Id = "SelectedHeatNetworkType-3",
                    Value = ApiHeatNetworkType.DeveloperLedDistrictHeatNetworkSm.ToString(),
                    Text = "Developer‑led District Heat Network(small)",
                    Hint = "An Energy Centre which is being developed as part of a new build developer-led District Heating Network",
                    SummaryText = "Help with developer-led District Heat Networks",
                    DetailsText = "A developer-led District Heating Network refers to a heat network that is applicable where there are only a small number of new build apartment blocks or other Consumer Connections, with small amounts of District Distribution Network pipework. For example, a District Heating System where there are only two buildings, with one small length of buried pipework connecting them together."
                },
                new() {
                    Id = "SelectedHeatNetworkType-4",
                    Value = ApiHeatNetworkType.CommunalHeatNetwork.ToString(),
                    Text = "Communal Heat Network",
                    Hint = "An Energy Centre which is being developed as part of a new build Communal Heat Network",
                    SummaryText = "Help with Communal Heat Networks",
                    DetailsText = "A Communal Heat Network is a heat network which serves a single building divided into separate premises or persons in those premises (e.g. habitable dwellings).\r\n\r\nThis will most likely be a developer lead heat network, where the heat network is built to serve a single development.\r\n\r\nFor example, an Energy Centre serving a heat network in a single building containing multiple apartments and a small number of commercial connections. The Energy Centre (and rest of the heat network) is constructed simultaneously to the rest of the building."
                }
            };
            return heatNetworkOptions;
        }



        /// <summary>
        /// Validates that input is a comma-separated integer list, e.g., "10" or "10, 11".
        /// Parses to List<int>.
        /// </summary>
        public static bool TryParseIntArray(string? input, out List<int> values, out string error)
        {
            values = new List<int>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Enter integers separated by commas, e.g., 10 or 10, 11.";
                return false;
            }

            var parts = input.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                error = "Enter integers separated by commas, e.g., 10 or 10, 11.";
                return false;
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    error = "Found an empty value. Use format like 10 or 10, 11 (no trailing commas).";
                    return false;
                }

                // Integer-only
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    error = $"'{part}' is not a valid integer. Use format like 10 or 10, 11.";
                    return false;
                }

                values.Add(i);
            }

            return true;
        }

        /// <summary>
        /// Validates that input is a comma-separated number list (ints or decimals), e.g., "10", "10, 11.5".
        /// Parses to List<decimal> for precision.
        /// </summary>
        /// <remarks>
        /// Uses invariant culture: decimal point is '.' (e.g., 11.5). Thousands separators are not allowed.
        /// </remarks>
        public static bool TryParseIntNumber(string? input, out List<decimal> values, out string error)
        {
            values = new List<decimal>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Required. Enter numbers separated by commas, e.g., 10 or 10, 11.5.";
                return false;
            }

            var parts = input.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                error = "Enter numbers separated by commas, e.g., 10 or 10, 11.5.";
                return false;
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    error = "Found an empty value. Use format like 10 or 10, 11.5 (no trailing commas).";
                    return false;
                }

                // Numbers: integers or decimals. InvariantCulture with '.' as decimal separator.
                if (!decimal.TryParse(part, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                {
                    error = $"'{part}' is not a valid number. Use format like 10 or 10, 11.5 (decimal point is '.').";
                    return false;
                }

                values.Add(d);
            }

            return true;
        }


        public static List<NetworkDetailsOption> GetDefaultNetworkDetailsOptions()
        {
            return new List<NetworkDetailsOption>
            {
                new() { Id = NetworkDetailsType.NetworkCharacteristics, Label = "Network characteristics", Hint = "", UiStatus = StatusConstants.ReadyToStart, IsEnabled = true },
                new() { Id = NetworkDetailsType.NetworkElements, Label = "Network elements", Hint = "", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                new() { Id = NetworkDetailsType.Soa, Label = "Element Statement of Applicability", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                new() { Id = NetworkDetailsType.MeteringAndMonitoringStrategy, Label = "Metering and monitoring strategy", UiStatus = StatusConstants.Incomplete, IsEnabled = true },
                new() { Id = NetworkDetailsType.AssessmentPlan, Label = "Assessment plan", UiStatus = StatusConstants.Incomplete, IsEnabled = true },
                new() { Id = NetworkDetailsType.DesignConstructionLog, Label = "Design and construction log", UiStatus = StatusConstants.Incomplete, IsEnabled = true }
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

        public static List<NetworkElementOption> GetDefaultNetworkElementOptions()
        {
            return new List<NetworkElementOption>
            {
                new() { Id = HeatNetworkElementDisplayType.EnergyCentre, Label = "Energy Centre", Hint = "Only 1 allowed per heat network unless part of a closed loop." },
                new() { Id = HeatNetworkElementDisplayType.DistributionNetwork, Label = "District Distribution Network", Hint = "Only 1 allowed per heat network." },
                new() { Id = HeatNetworkElementDisplayType.ThermalSubStation, Label = "Thermal Substation" },
                new() { Id = HeatNetworkElementDisplayType.CommunalDistributionNetwork, Label = "Communal Distribution Network"},
                new() { Id = HeatNetworkElementDisplayType.ConsumerConnections, Label = "Consumer Connections" },
                new() { Id = HeatNetworkElementDisplayType.ConsumerHeatSystems, Label = "Consumer Heat Systems" }
            };
        }

        public static string GetNetworkElementIdByType(string elementType)
        {
            return elementType switch
            {
                "EnergyCentre" => "EC",
                "DistributionNetwork" => "DDN",
                "ThermalSubStation" => "TS",
                "CommunalDistributionNetwork" => "CDN",
                "ConsumerConnections" => "CC",
                "ConsumerHeatSystems" => "CHS",
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), $"Not expected heat network element type value: {elementType}")
            };

        }

        public static HeatNetworkElementDisplayType GetNetworkElementDisplayTypeById(string elementId)
        {
            return elementId switch
            {
                "EC" => HeatNetworkElementDisplayType.EnergyCentre,
                "DDN" => HeatNetworkElementDisplayType.DistributionNetwork,
                "TS" => HeatNetworkElementDisplayType.ThermalSubStation,
                "CDN" => HeatNetworkElementDisplayType.CommunalDistributionNetwork,
                "CC" => HeatNetworkElementDisplayType.ConsumerConnections,
                "CHS" => HeatNetworkElementDisplayType.ConsumerHeatSystems,
                _ => throw new ArgumentOutOfRangeException(nameof(elementId), $"Not expected heat network element ID value: {elementId}")
            };
        }
    }
}