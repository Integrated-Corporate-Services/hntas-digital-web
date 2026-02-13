using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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
                new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centre", Hint = "Only 1 allowed per heat network unless part of a closed loop." },
                new() { Id = HeatNetworkElementType.DistributionNetwork, Label = "Distribution network", Hint = "Only 1 allowed per heat network." },
                new() { Id = HeatNetworkElementType.ThermalSubStation, Label = "Thermal sub station" },
                new() { Id = HeatNetworkElementType.CommunalDistributionNetwork, Label = "Communal distribution network"},
                new() { Id = HeatNetworkElementType.ConsumerConnections, Label = "Consumer connections" },
                new() { Id = HeatNetworkElementType.ConsumerHeatSystems, Label = "Consumer heat systems" }
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
                new() { Id = NetworkDetailsType.MeteringAndMonitoringStrategy, Label = "Metering and monitoring strategy", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                new() { Id = NetworkDetailsType.AssessmentPlan, Label = "Assessment plan", UiStatus = StatusConstants.CannotStartYet, IsEnabled = false },
                new() { Id = NetworkDetailsType.DesignConstructionLog, Label = "Design and construction log", UiStatus = StatusConstants .CannotStartYet, IsEnabled = false }
            };
        }

        public static void UpdateOptionStatus<TStatus>(NetworkDetailsOption option, TStatus? status)
    where TStatus : struct, Enum
        {
            if (status == null)
                return;

            var statusName = Enum.GetName(typeof(TStatus), status.Value);

            option.UiStatus = statusName switch
            {
                "Complete" => StatusConstants.Completed,
                "InProgress" => StatusConstants.InProgress,
                "ReadyToStart" => StatusConstants.ReadyToStart,
                _ => StatusConstants.NotStarted
            };

            option.IsEnabled = statusName != "Submitted";
        }        
    }
}