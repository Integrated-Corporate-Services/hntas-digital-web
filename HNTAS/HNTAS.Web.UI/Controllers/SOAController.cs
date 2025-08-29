using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using ApiHeatNetworkType = HNTAS.Api.Client.Model.HeatNetworkType;

namespace HNTAS.Web.UI.Controllers
{
    public class SOAController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly ISoaProjectService _soaProjectService;

        public SOAController(ISessionHelper sessionHelper, ISoaProjectService soaProjectService)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
        }

        [HttpGet]
        public IActionResult SOAIntro()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("Details", "HeatNetwork", new { hnid = hnId });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSOAIntroAsync()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            if (hnId == null)
            {
                return BadRequest();
            }
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);
            soaProject ??= await _soaProjectService.CreateAsync(hnId);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaProjectId, soaProject.Id);
            return RedirectToAction("HeatNetworkType");
        }

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("SOAIntro");
            var model = new HeatNetworkTypeViewModel
            {
                HeatNetworkTypes = GetHeatNetworkTypeOptions()
            };
            return View(model);
        }

        private List<SelectItemOption> GetHeatNetworkTypeOptions()
        {
            var heatNetworkOptions = new List<SelectItemOption>
            {
                new() {
                    Value = ApiHeatNetworkType.CityScaleDistrictHeatingNetwork.ToString(),
                    Text = "City-scale district heating network (CSDH)",
                    Hint = "Connects multiple buildings independently, with third-party connections."
                },
                new() {
                    Value = ApiHeatNetworkType.DevelopmentLedDistrictHeatingNetwork.ToString(),
                    Text = "Development led district heating network (DLDH)",
                    Hint = "Constructed simultaneously with wider building works."
                },
                new() {
                    Value = ApiHeatNetworkType.LargeCommunalHeatNetwork.ToString(),
                    Text = "Large communal heat network (c.300 consumers)",
                    Hint = "Serves multiple buildings within one development."
                },
                new() {
                    Value = ApiHeatNetworkType.MediumCommunalHeatNetwork.ToString(),
                    Text = "Medium communal heat network (c.100 consumers)",
                    Hint = "Serves multiple buildings within one development."
                },
                new() {
                    Value = ApiHeatNetworkType.SmallCommunalHeatNetwork.ToString(),
                    Text = "Small communal heat network (c.50 consumers)",
                    Hint = "Serves consumers within a single building."
                },
                new() {
                    Value = ApiHeatNetworkType.Other.ToString(),
                    Text = "Other"
                }
            };
            return heatNetworkOptions;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitHeatNetworkTypeAsync(HeatNetworkTypeViewModel model)
        {
            if (model.SelectedHNType == "Other" && string.IsNullOrWhiteSpace(model.OtherNetworkDescription))
            {
                ModelState.AddModelError(nameof(model.OtherNetworkDescription), "Please describe your network type.");
            }

            if (!ModelState.IsValid)
            {
                this.ShowBackButton("SOAIntro");
                model.HeatNetworkTypes = GetHeatNetworkTypeOptions();
                return View("HeatNetworkType", model);
            }

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ApiHeatNetworkType hnype = (ApiHeatNetworkType)Enum.Parse(typeof(ApiHeatNetworkType), model.SelectedHNType);

            await _soaProjectService.UpdateNetworkTypeAsync(hnId, new Api.Client.Model.NetworkTypeSelection2(type: hnype, otherNetworkDescription: model.OtherNetworkDescription));

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
            return RedirectToAction("HeatNetworkSOADetails");
        }

        [HttpGet]
        public IActionResult HeatNetworkSOADetails()
        {
            this.ShowBackButton("NetworkConnectionType");
            var model = new StepByStepGuideModel
            {
                Steps = StaticSoaSteps.GetSteps(SoaSteps.ChooseElements, Url)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult SelectElements()
        {
            this.ShowBackButton("HeatNetworkSOADetails");
            return View();
        }

        [HttpPost]
        public IActionResult SubmitSelectedElements()
        {
            return RedirectToAction("InitialSoa");
        }

        [HttpGet]
        public IActionResult InitialSoa()
        {
            this.ShowBackButton("ElementsOfHeatNetwork");
            var model = new StepByStepGuideModel
            {
                Steps = StaticSoaSteps.GetSteps(SoaSteps.InitialSoa, Url)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ElementList()
        {
            this.ShowBackButton("InitialSoa");

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

        public IActionResult DefineSoa()
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
                Pathway = "new build (stage 1-7)",
                Steps = StaticSoaSteps.GetSteps(SoaSteps.DefineSoa, Url)
            };
            return View(model);
        }

        public IActionResult DefineSoaDetails(int phaseIndex = 0)
        {
            var model = new StatementOfApplicabilityViewModel
            {
                ProjectName = "Olympic Park Aberdeen",
                PageTitle = "Define SOA – add details to your statement of applicability (SOA)",
                CurrentPhaseIndex = phaseIndex,
                Phases = new List<PhaseViewModel>
                {
                    new()
                    {
                        Name = "Phase 1",
                        Title = "Feasibility",
                        IsActive = true,
                        Stages = new List<StageViewModel>
                        {
                            new()
                            {
                                Name = "Stage 1 – concept design",
                                Elements = new List<ElementViewModel>
                                {
                                    new() { Name = "Energy centre", Status = "Not yet started", StatusClass = "govuk-tag--grey", Url = Url.Action("UploadSOAElementDocuments", "Soa", new { phase = 1, elementName = "energy-centre" }) },
                                    new() { Name = "Thermal sub station", Status = "Not yet started", StatusClass = "govuk-tag--grey", Url = Url.Action("UploadSOAElementDocuments", "Soa", new { phase = 1, elementName = "thermal-sub-station" }) },
                                    new() { Name = "Communal distribution network", Status = "Not yet started", StatusClass = "govuk-tag--grey", Url = Url.Action("AddDetails", "Soa", new { phase = 1, elementName = "communal-distribution-network" }) },
                                    new() { Name = "Consumer connections", Status = "Not yet started", StatusClass = "govuk-tag--grey", Url = Url.Action("AddDetails", "Soa", new { phase = 1, elementName = "consumer-connections" }) }
                                }
                            }
                        }
                    },
                    // Add Phase 2–5 similarly
                    new PhaseViewModel { Name = "Phase 2", Title = "Design", Stages = new List<StageViewModel>() },
                    new PhaseViewModel { Name = "Phase 3", Title = "Construction", Stages = new List<StageViewModel>() },
                    new PhaseViewModel { Name = "Phase 4", Title = "Testing", Stages = new List<StageViewModel>() },
                    new PhaseViewModel { Name = "Phase 5", Title = "Handover", Stages = new List<StageViewModel>() }
                },
            };

            return View(model);
        }


        public IActionResult UploadSOAElementDocuments(string elementName)
        {
            var model = new UploadSOAElementDocumentsViewModel
            {
                PageTitle = "Upload SOA Documents",
                ElementDescription = "Upload your SOA for each element."
            };

            if (elementName == "energy-centre")
            {
                model.ElementName = "Energy centre";
                model.Documents = new List<DocumentUploadModel>
                    {
                        new DocumentUploadModel { Name = "Primary energy centre", FileInputId = "primary-soa-upload", IsRequired = true },
                        new DocumentUploadModel { Name = "Secondary energy centre", FileInputId = "secondary-soa-upload", IsRequired = false }
                    };
            }
            else if (elementName == "thermal-sub-station")
            {
                model.ElementName = "Thermal sub station";
                model.Documents = new List<DocumentUploadModel>
                {
                    new DocumentUploadModel { Name = "Thermal sub station 1", FileInputId = "thermal-soa-upload-1", IsRequired = true },
                    new DocumentUploadModel { Name = "Thermal sub station 2", FileInputId = "thermal-soa-upload-1", IsRequired = true }
                };
            }

            return View(model);
        }

    }
}
