using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using ApiHeatNetworkType = HNTAS.Api.Client.Model.HeatNetworkType;

namespace HNTAS.Web.UI.Controllers
{
    public class SOAController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly ISoaProjectService _soaProjectService;
        private readonly ILogger<SOAController> _logger;
        private readonly IS3UploadService _s3UploadService;

        public SOAController(ISessionHelper sessionHelper,
            ISoaProjectService soaProjectService,
            ILogger<SOAController> logger,
            IS3UploadService s3UploadService)
        {
            _sessionHelper = sessionHelper;
            _soaProjectService = soaProjectService;
            _logger = logger;
            _s3UploadService = s3UploadService;
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
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            if (hnId == null || userId == null)
            {
                return BadRequest();
            }
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);
            soaProject ??= await _soaProjectService.CreateAsync(hnId, userId);

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SoaProjectId, soaProject.Id);
            return RedirectToAction("HeatNetworkType");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkTypeAsync()
        {
            this.ShowBackButton("SOAIntro");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);
            //convert soaProject to HeatNetworkTypeViewModel

            var model = new HeatNetworkTypeViewModel
            {
                HeatNetworkTypes = GetHeatNetworkTypeOptions()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitHeatNetworkTypeAsync(HeatNetworkTypeViewModel model)
        {
            if (model.SelectedHNType == ApiHeatNetworkType.Other && string.IsNullOrWhiteSpace(model.OtherNetworkDescription))
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
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            await _soaProjectService.UpdateNetworkTypeAsync(hnId, userId, new NetworkTypeSelection2(type: model.SelectedHNType, otherNetworkDescription: model.OtherNetworkDescription));

            return RedirectToAction("NetworkConnectionType");
        }

        [HttpGet]
        public IActionResult NetworkConnectionType()
        {
            this.ShowBackButton("HeatNetworkType");

            var model = new NetworkConnectionTypeViewModel
            {
                ConnectionTypes = GetConnectionTypeOptions()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitNetworkConnectionTypeAsync(NetworkConnectionTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("HeatNetworkType");
                model.ConnectionTypes = GetConnectionTypeOptions();
                return View("NetworkConnectionType", model);
            }

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            await _soaProjectService.UpdateConnectionsAsync(hnId, userId, model.SelectedConnections);
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
            var model = new HeatNetworkElementViewModel()
            {
                ElementOptions = Utility.GetElementOptions()
            };

            return View(model);
        }
        
        [HttpPost]
        public IActionResult SubmitSelectedElements(HeatNetworkElementViewModel model)
        {
            model.ElementOptions = Utility.GetElementOptions();

            if (!ModelState.IsValid)
            {
                return View("SelectElements", model);
            }

            // Custom validation: ensure quantity is entered for each selected element
            foreach (var selectedId in model.SelectedElementIds)
            {
                if (!model.ElementCounts.TryGetValue(selectedId, out var count) || count == null || count <= 0)
                {
                    var element = Utility.GetElementOptions().FirstOrDefault(x => x.Id == selectedId);
                    if (element == null)
                    {
                        return BadRequest();
                    }
                    ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter number of {element.Label}.");
                }
            }

            if (!ModelState.IsValid)
            {

                return View("SelectElements", model);
            }

            List<HeatNetworkElement> heatNetworkElements = new List<HeatNetworkElement>();

            foreach (var selectedElement in model.SelectedElementIds)
            {
                heatNetworkElements.Add(new HeatNetworkElement(selectedElement, model.ElementCounts[selectedElement].Value));
            }

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            _soaProjectService.UpdateNetworkElements(hnId, userId, heatNetworkElements);

            return RedirectToAction("InitialSoa");
        }

        [HttpGet]
        public async Task<IActionResult> InitialSoa()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("SelectElements");
            //get soa from db
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);

            if (soaProject == null)
            {
                return BadRequest();
            }

            List<SelectedElement> networkElements = new List<SelectedElement>();
            foreach (var element in soaProject.JourneyData.HeatNetworkElements)
            {
                networkElements.Add(new SelectedElement
                {
                    Count = element.Count ?? 0,
                    Name = Utility.GetElementOptions()?.FirstOrDefault(e => e.Id == element.Name)?.Label ?? string.Empty
                });
            }

            var model = new InitialSoaViewModel
            {
                Steps = StaticSoaSteps.GetSteps(SoaSteps.InitialSoa, Url),
                SelectedElements = networkElements
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ElementList()
        {
            this.ShowBackButton("InitialSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);

            if (soaProject?.JourneyData?.HeatNetworkElements == null)
            {
                return View(new ElementListViewModel
                {
                    HeatNetworkName = hnName,
                    HnId = hnId,
                    Elements = new List<ElementListItem>()
                });
            }

            var orderedTypes = new List<HeatNetworkElementType>
            {
                HeatNetworkElementType.EnergyCentre,
                HeatNetworkElementType.DistributionNetwork,
                HeatNetworkElementType.ThermalSubStation,
                HeatNetworkElementType.CommunalDistributionNetwork,
                HeatNetworkElementType.ConsumerConnections,
                HeatNetworkElementType.ConsumerHeatSystems
            };

            var elements = new List<ElementListItem>();
            bool previousCompleted = true;

            foreach (var type in orderedTypes)
            {
                var element = soaProject.JourneyData.HeatNetworkElements.FirstOrDefault(e => e.Name == type);
                var count = element?.Count ?? 0;

                if (count == 0)
                {
                    continue;
                }

                var hasData = element?.Locations?.Any() == true;
                var allFilled = element?.Locations?.Count == count && element.Locations.All(loc => !string.IsNullOrWhiteSpace(loc));

                bool isEnabled = elements.Count == 0 || previousCompleted;

                string uiStatus;

                if (allFilled)
                {
                    uiStatus = UiStatusConstants.Completed;
                    previousCompleted = true;
                }
                else if (isEnabled && hasData)
                {
                    uiStatus = UiStatusConstants.InProgress;
                    previousCompleted = false;
                }
                else if (isEnabled)
                {
                    uiStatus = UiStatusConstants.NotStarted;
                    previousCompleted = false;
                }
                else
                {
                    uiStatus = UiStatusConstants.CannotStartYet;
                    previousCompleted = false;
                }

                elements.Add(new ElementListItem
                {
                    ElementType = type,
                    Name = Utility.GetElementOptions().FirstOrDefault(x => x.Id == type).Label ?? string.Empty,
                    Count = count,
                    UiStatus = uiStatus,
                    IsEnabled = isEnabled
                });
            }

            var model = new ElementListViewModel
            {
                HeatNetworkName = hnName,
                HnId = soaProject.HnId,
                Elements = elements
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EnterElementLocationsAsync(string elementName)
        {
            this.ShowBackButton("ElementList");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);
            var selectedElement = Utility.GetElementOptions().FirstOrDefault(x => x.Id.ToString().ToLower() == elementName.ToLower());
            var element = soaProject.JourneyData.HeatNetworkElements.FirstOrDefault(x => x.Name == selectedElement.Id);

            var model = new EnterElementLocationsViewModel
            {
                ElementName = selectedElement?.Label ?? string.Empty,
            };

            model.Locations = Enumerable.Repeat(string.Empty, element.Count.Value).ToList();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEnterElementLocations(EnterElementLocationsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //_logger.LogWarning("Invalid location input for element: {ElementName}", model.ElementName);
                return View("EnterElementLocations", model);
            }

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var elementType = Utility.GetElementOptions().FirstOrDefault(x => x.Id.ToString().ToLower() == model.ElementName.ToLower()).Id;
            model.Locations = model.Locations.Where(x => !string.IsNullOrEmpty(x)).ToList();
            await _soaProjectService.UpdateElementLocations(new UpdateElementLocationsRequest(hnId, userId, elementType, model.Locations));

            //_logger.LogInformation("Saving {Count} locations for element: {ElementName}", model.Locations.Count, model.ElementName);

            return RedirectToAction("ElementList");
        }


        public async Task<IActionResult> DefineSoaAsync()
        {

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnNameId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);

            if (soaProject == null)
            {
                return BadRequest();
            }

            List<SelectedElement> networkElements = new List<SelectedElement>();
            foreach (var element in soaProject.JourneyData.HeatNetworkElements)
            {
                networkElements.Add(new SelectedElement
                {
                    Count = element.Count ?? 0,
                    Name = Utility.GetElementOptions()?.FirstOrDefault(e => e.Id == element.Name)?.Label ?? string.Empty
                });
            }

            var model = new SoADetailsViewModel
            {
                SelectedElements = networkElements,
                HeatNetworkName = hnNameId,
                Pathway = "1",
                Steps = StaticSoaSteps.GetSteps(SoaSteps.DefineSoa, Url)
            };

            this.ShowBackButton("ElementList");
            return View(model);
        }

        public async Task<IActionResult> DefineSoaDetailsAsync(int phaseIndex = 0, int pathway = 3)
        {
            this.ShowBackButton("DefineSoa");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);

            if (soaProject == null)
            {
                return BadRequest();
            }

            List<SelectedElement> networkElements = new List<SelectedElement>();
            foreach (var element in soaProject.JourneyData.HeatNetworkElements)
            {
                networkElements.Add(new SelectedElement
                {
                    Count = element.Count ?? 0,
                    Name = Utility.GetElementOptions()?.FirstOrDefault(e => e.Id == element.Name)?.Label ?? string.Empty
                });
            }

            var model = new StatementOfApplicabilityViewModel
            {
                ProjectName = "Olympic Park Aberdeen",
                PageTitle = "Define SOA – add details to your statement of applicability (SOA)",
                Pathway = pathway,
                CurrentPhaseIndex = phaseIndex,
                Phases = SoaPhaseStageMapping.Phases.Select((phase, index) => new PhaseViewModel
                {
                    Name = phase.Name,
                    Title = phase.Title,
                    IsActive = index == phaseIndex,
                    Stages = phase.Stages.Select(stage => new StageViewModel
                    {
                        Name = stage.Name,
                        Elements = GetDefaultElementsForStage(stage.SoaStage, index + 1, soaProject)
                    }).ToList()
                }).ToList()
            };

            return View(model);
        }

        private List<ElementViewModel> GetDefaultElementsForStage(SoaStage stage, int phaseNumber, SoaProject soaProject)
        {
            var stageNumber = (int)stage;
            var phaseEnum = (SoaPhase)phaseNumber;

            var networkElements = new List<ElementViewModel>();

            foreach (var element in soaProject.JourneyData.HeatNetworkElements)
            {
                var label = Utility.GetElementOptions()?.FirstOrDefault(e => e.Id == element.Name)?.Label ?? string.Empty;

                var matchingDocs = element.Documents
                    .Where(d => d.Phase == phaseEnum && d.Stage == stage)
                    .ToList();

                var status = matchingDocs.Count == 0
                    ? UiStatusConstants.NotStarted
                    : matchingDocs.Count < element.Count
                        ? UiStatusConstants.InProgress
                        : UiStatusConstants.Completed;

                networkElements.Add(new ElementViewModel
                {
                    Name = label,
                    Status = status,
                    Url = Url.Action("UploadSOAElementDocuments", "Soa", new
                    {
                        phase = phaseNumber,
                        stage = stageNumber,
                        elementName = element.Name.ToString()
                    })
                });
            }

            return networkElements;
        }



        [HttpGet]
        public async Task<IActionResult> UploadSOAElementDocuments(string elementName, int phase, int stage)
        {
            this.ShowBackButton("DefineSoaDetails");

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var soaProject = await _soaProjectService.GetByHnIdAsync(hnId);

            var selectedElement = Utility.GetElementOptions().FirstOrDefault(x =>
                x.Id.ToString().Equals(elementName, StringComparison.OrdinalIgnoreCase));

            if (selectedElement == null)
            {
                //_logger.LogWarning("Element not found for name: {ElementName}", elementName);
                return NotFound();
            }

            var element = soaProject.JourneyData.HeatNetworkElements
                .FirstOrDefault(x => x.Name == selectedElement.Id);

            var model = new UploadSOAElementDocumentsViewModel
            {
                PageTitle = "Upload SOA Documents",
                Phase = phase,
                Stage = stage,
                ElementName = selectedElement.Label,
                ElementDescription = "Upload your SOA for each element.",
                Documents = BuildDocumentInputsForElement(selectedElement.Id, element?.Count ?? 0)
            };

            return View(model);
        }

        private List<DocumentUploadModel> BuildDocumentInputsForElement(HeatNetworkElementType elementId, int count)
        {
            var elementLabel = Utility.GetElementOptions().FirstOrDefault(x => x.Id == elementId)?.Label;

            var documents = new List<DocumentUploadModel>();

            for (int i = 1; i <= count; i++)
            {
                documents.Add(new DocumentUploadModel
                {
                    Name = $"{elementLabel} {i}",
                    FileInputId = $"{elementLabel.ToLower().Replace(" ", "-")}-soa-upload-{i}",
                    IsRequired = true
                });
            }

            return documents;
        }

        [HttpPost]
        public async Task<IActionResult> SaveUploadedSOAElementDocuments(string elementName, int phase, int stage)
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            var selectedElement = Utility.GetElementOptions().FirstOrDefault(x => x.Label.ToLower() == elementName.ToLower());

            if (selectedElement == null)
            {
                //_logger.LogWarning("Invalid element name: {ElementName}", elementName);
                return NotFound();
            }

            var uploadedDocuments = new List<UploadedDocument>();

            foreach (var key in Request.Form.Files.Select(f => f.Name))
            {
                var file = Request.Form.Files[key];
                if (file != null && file.Length > 0)
                {
                    var s3Key = await _s3UploadService.UploadFileAsync(file, $"soa/{hnId}/{phase}/{stage}/{selectedElement.Id}");
                    uploadedDocuments.Add(new UploadedDocument
                    {
                        FileName = file.FileName,
                        S3Key = s3Key,
                        Phase = (SoaPhase)phase, // You can dynamically resolve this if needed
                        Stage = (SoaStage)stage, // Same here
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    });
                }
            }

            if (uploadedDocuments.Any())
            {
                var request = new UpdateElementDocumentsRequest
                {
                    HnId = hnId,
                    ElementType = selectedElement.Id,
                    UpdatedBy = userId,
                    Documents = uploadedDocuments
                };

                await _soaProjectService.UpdateElementDocuments(request);
                // _logger.LogInformation("Saved {Count} documents for element {ElementName} in HN ID: {HnId}", uploadedDocuments.Count, elementName, hnId);
            }
            else
            {
                // _logger.LogWarning("No valid files uploaded for element {ElementName} in HN ID: {HnId}", elementName, hnId);
            }

            return RedirectToAction("DefineSoaDetails");
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


        private List<SelectItemOption> GetConnectionTypeOptions()
        {
            var companyTypeOptions = new List<SelectItemOption>
            {
                new() {
                    Value = ConnectionType.ChildConnections.ToString(),
                    Text = "Child connections (Are you supplying any other networks)",
                    Hint = "Are you supplying any other district HN?"
                },
                new() {
                    Value = ConnectionType.CommunalHeatNetworkConnection.ToString(),
                    Text = "Communal heat network connection",
                    Hint = "Are you supplying residential communally heated blocks"
                },
                new() {
                    Value = ConnectionType.CommercialConnection.ToString(),
                    Text = "Commercial connection (hotel, office)",
                    Hint = "Are you supplying any other large public/commercial buildings (office, hotel, retail)"
                },
                new() {
                    Value = ConnectionType.ParentConnection.ToString(),
                    Text = "Parent connection (Are you being supplied by another network)",
                    Hint = "Are you being supplied by a district HN"
                }
            };
            return companyTypeOptions;
        }

    }
}
