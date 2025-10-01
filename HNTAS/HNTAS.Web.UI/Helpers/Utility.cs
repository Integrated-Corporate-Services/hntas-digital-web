using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;

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

            switch (userRole)
            {
                case "RegulatoryContact":
                    return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedDesigner).ToString(), Text = "Designated designer" },
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedContractor).ToString(), Text = "Designated contractor" },
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedOperator).ToString(), Text = "Designated operator" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
                case "DesignatedDesigner":
                    return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingDesigner).ToString(), Text = "Contributing designer" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
                case "DesignatedContractor":
                    return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingContractor).ToString(), Text = "Contributing contractor" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
                case "DesignatedOperator":
                    return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.ContributingOperator).ToString(), Text = "Contributing operator" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
                case "Assessor":
                    return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
                default:
                    return null;
            }
        }

        public static async Task<string> GetUserRoleByUserHNMapping(UserResponse user, string hnId)
        {
            var userRole = "";
            if (user?.Roles?.Contains(Api.Client.Model.UserRole.RegulatoryContact) == true)
            {
                userRole = Api.Client.Model.UserRole.RegulatoryContact.ToString();
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

        public static async Task<List<SelectItemOption>?> GetHeatNetworkSelectListAsync(List<HeatNetworkResponse> response)
        {
            //var response = await _userService.GetUserHeatNetworks(userId);
            if (response == null) return null;

            return response.Select(hn => new SelectItemOption
            {
                Value = hn.HnId,
                Text = hn.Name
            }).ToList();
        }
    }
}