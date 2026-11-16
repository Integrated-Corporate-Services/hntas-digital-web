using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

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
        public static void ShowBackButton(this Controller controller, string action, string controllerName, object? routeValues = null)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, routeValues);
        }

        public static void ShowBackButton(this Controller controller, string action, string controllerName, string fragement)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, null, "", "", fragement);
        }

        public static string CapitalizeCommaSeparated(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.Split(',')
                             .Select(w => w.Trim())
                             .Where(w => !string.IsNullOrEmpty(w))
                             .ToList();

            if (words.Count == 0)
                return string.Empty;

            var processedWords = words.Select((w, index) =>
                index == words.Count - 1
                    ? w.ToUpper()
                    : char.ToUpper(w[0]) + w.Substring(1).ToLower());

            return string.Join(", ", processedWords);

        }        

        public static List<SelectItemOption> GetContributorSelectList(string userRole)
        {
            if (userRole == UserRole.ResponsibleParty.ToString() || userRole == UserRole.NetworkManager.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedDutyHolder).ToString(), Text = "Designated duty holder" },
                        new SelectItemOption { Value = ((int)ContributorRole.Contributor).ToString(), Text = " Contributor" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.DesignatedDutyHolder.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.Contributor).ToString(), Text = "Contributor" },
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
            if (user?.Roles?.Contains(UserRole.ResponsibleParty) == true)
            {
                userRole = UserRole.ResponsibleParty.ToString();
            }
            else if (user?.Roles?.Contains(UserRole.NetworkManager) == true)
            {
                userRole = UserRole.NetworkManager.ToString();
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
        
    }
}