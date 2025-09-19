using HNTAS.Api.Client.Model;
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
    }
}