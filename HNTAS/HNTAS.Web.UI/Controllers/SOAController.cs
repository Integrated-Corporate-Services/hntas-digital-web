using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Models.Soa.test;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class SOAController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        public SOAController(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        public IActionResult SOAIntro()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("Details", "HeatNetwork", new { hnid = hnId });
            return View();
        }

        [HttpPost]
        public IActionResult SubmitSOAIntro()
        {
            return RedirectToAction("HeatNetworkType");
        }

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("SOAIntro");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitHeatNetworkType()
        {
            return RedirectToAction("NetworkConnectionType");
        }

        [HttpGet]
        public IActionResult NetworkConnectionType()
        {
            this.ShowBackButton("HeatNetworkType");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitNetworkConnectionType()
        {
            return RedirectToAction("DefineSOA");
        }

        [HttpGet]
        public IActionResult DefineSOA()
        {
            this.ShowBackButton("NetworkConnectionType");
            return View();
        }

        [HttpGet]
        public IActionResult ElementsOfHeatNetwork()
        {
            this.ShowBackButton("DefineSOA");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitElementsOfHeatNetwork()
        {
            return RedirectToAction("AddDetailsForElements");
        }

        [HttpGet]
        public IActionResult AddDetailsForElements()
        {
            this.ShowBackButton("ElementsOfHeatNetwork");
            return View();
        }

        [HttpGet]
        public IActionResult ElementList()
        {
            this.ShowBackButton("AddDetailsForElements");

            var model = new ElementListViewModel
            {
                HeatNetworkName = "Olympic Park Aberdeen",
                EnergyCentreCount = 2,
                ThermalSubStationCount = 2,
                CommunalDistributionNetworkCount = 1,
                ConsumerConnectionsCount = 10
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult EnergyCentre()
        {
            this.ShowBackButton("ElementList");
            var model = new EnergyCentreViewModel
            {
                PrimaryEnergyCentreLocation = "https://what3words.com/pretty.needed.chill"
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult ThermalSubstation()
        {
            this.ShowBackButton("ElementList");
            var model = new ThermalSubstationViewModel
            {
                PrimaryThermalSubstationLocation = "https://what3words.com/pretty.needed.chill"
            };
            return View(model);
        }

        public IActionResult DefineSoaDetails()
        {
            var model = new SoADetailsViewModel
            {
                SelectedElements = new List<SelectedElement>
                {
                    new SelectedElement { Name = "Energy Centre", Count = 2 },
                    new SelectedElement { Name = "Thermal sub station", Count = 2 },
                    new SelectedElement { Name = "Communal distribution network", Count = 1 },
                    new SelectedElement { Name = "Consumer connections", Count = 10 }
                },
                HeatNetworkName = "Heat Network 1",
                Pathway = "new build (stage 1-7)"
            };
            return View(model);
        }

        public IActionResult DefineSoaStepByStep()
        {
            var model = new StepByStepGuideModel
            {
                Steps = new List<StepDetail>
                {
                    new() { StepNumber = 1, Title = "Choose your elements", Url = Url.Action("SelectElements", "Soa") },
                    new() { StepNumber = 2, Title = "Initial SOA", Url = Url.Action("InitialSoa", "Soa") },
                    new() { StepNumber = 3, Title = "Define SOA", Url = Url.Action("DefineSoa", "Soa"), IsCurrent = true, IsExpanded = true },
                    new() { StepNumber = 4, Title = "Add assessment plan", Url = Url.Action("AssessmentPlan", "Soa") },
                    new() { StepNumber = 5, Title = "Submit your SOA", Url = Url.Action("SubmitSoa", "Soa") }
                }
            };

            return View(model);
        }
    }
}
