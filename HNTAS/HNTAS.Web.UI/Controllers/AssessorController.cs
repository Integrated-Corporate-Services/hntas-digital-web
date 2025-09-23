using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    public class AssessorController : Controller
    {
        private readonly ILogger<AssessorController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IUserService _userService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly ISoaService _soaService;
        private readonly IS3UploadService _s3UploadService;


        public AssessorController(ILogger<AssessorController> logger, ISessionHelper sessionHelper, IUserService userService, IHeatNetworkService heatNetworkService, ISoaService soaProjectService, IS3UploadService s3UploadService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _userService = userService;
            _heatNetworkService = heatNetworkService;
            _soaService = soaProjectService;
            _s3UploadService = s3UploadService;
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            try
            {
                var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));

                if (user == null)
                {
                    throw new Exception("Unable to retrieve user information. Please try again later.");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user details.");
                throw;
            }
        }

        [HttpGet]
        public IActionResult DeclarationOfImpartiality([FromQuery] string hnid)
        {
            var hasDeclaredImpartiality = _heatNetworkService.GetAssessorImpartialityAsync(hnid.ToUpper()).Result;
            if(hasDeclaredImpartiality == true)
            {
                _sessionHelper.SaveToSession(HttpContext, "HasDeclaredImpartiality", "true");
                return RedirectToAction("HeatNetworkDetails", "Assessor", new { hnId = hnid.ToUpper() });
            }
            this.ShowBackButton("HeatNetworks", "UserManagement");
            var model = _sessionHelper.GetFromSession<DeclationOfImpartialityModel>(HttpContext, SessionKeys.DeclarationOfImpartialityModelKey) ?? new DeclationOfImpartialityModel();
            model.HnId = hnid.ToUpper();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclarationOfImpartiality(DeclationOfImpartialityModel model)
        {

            this.ShowBackButton("HeatNetworks", "UserManagement");
            if (!ModelState.IsValid || model.HasDeclaredImpartiality == false)
            {
                return View(model);
            }
            var setImpartiality = _heatNetworkService.SetAssessorImpartialityAsync(model.HnId.ToUpper()).Result;
            if (!setImpartiality)
            {
                ModelState.AddModelError(string.Empty, "There was a problem saving your declaration. Please try again.");
                return View(model);
            }
            _sessionHelper.SaveToSession(HttpContext, "HasDeclaredImpartiality", "true");
            return RedirectToAction("HeatNetworkDetails", "Assessor", new { hnId = model.HnId });
        }

        private List<ElementViewModel> GetElementsForStage(NullableOfSoaStage stage, int phaseNumber, SoaResponse soa)
        {
            var phaseEnum = (SoaPhase)phaseNumber;

            var networkElements = new List<ElementViewModel>();

            foreach (var element in soa.JourneyData.HeatNetworkElements)
            {

                var matchingDocs = element.Documents
                    .Where(d => d.Phase == phaseEnum.ToString() && d.Stage == stage.ToString())
                    .ToList();

                var status = matchingDocs.Count == 0
                    ? UiStatusConstants.NotStarted
                    : matchingDocs.Count < element.Count
                        ? UiStatusConstants.InProgress
                        : UiStatusConstants.Completed;

                networkElements.Add(new ElementViewModel
                {
                    Name = element.Name.ToString(),
                    Status = status,
                    Url = Url.Action("UploadSOAElementDocuments", "Soa", new
                    {
                        phase = phaseNumber,
                        stage = (int)stage,
                        elementName = element.Name.ToString()
                    }),
                    Count = element.Count
                });
            }

            return networkElements;
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkDetails([FromQuery] string hnid = "")
        {
            if(hnid == "")
            {
                hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            }
            this.ShowBackButton("HeatNetworks", "UserManagement");
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnid.ToUpper());
            try
            {
                var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
                var hnDetails = await _heatNetworkService.GetAsync(hnid?.ToUpper());

                if (user == null || hnDetails.Soa == null || hnDetails == null)
                {
                    return BadRequest();
                }
                var phaseIndex = 0;


                var model = new HeatNetworkDetailsViewModel
                {
                    HnId = hnDetails.HnId,
                    HnName = hnDetails.Name,
                    HnLocation = hnDetails.Location,
                    OrganisationName = user.Organisation.Name,
                    OrganisationAddress = user.Organisation.RegisteredAddress,
                    Pathway = hnDetails.Pathway,
                    CurrentPhaseIndex = phaseIndex,
                    Phases = SoaPhaseStageMapping.Phases.Skip(Convert.ToInt32(hnDetails.Pathway) - 1).ToList().Select((phase, index) => new PhaseViewModel
                    {
                        Name = phase.Name,
                        Title = phase.Title,
                        IsActive = index == phaseIndex,
                        Stages = phase.Stages.Select(stage => new StageViewModel
                        {
                            Name = stage.Name,
                            Elements = GetElementsForStage(stage.SoaStage, index + 1, hnDetails.Soa)
                        }).ToList()
                    }).ToList()
                };
                _sessionHelper.SaveToSession(HttpContext, "PhaseData", model.Phases);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving details.");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTheDocuments(int Phase, int Stage, string Element)
        {
            this.ShowBackButton("HeatNetworkDetails", "Assessor");
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var soaProject = await _soaService.GetByHnIdAsync(hnid.ToUpper());
            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnid.ToUpper());
            if (soaProject == null)
            {
                return BadRequest();
            }
            var documents = new List<UploadedDocument>();
            foreach (var hnElement in soaProject.JourneyData.HeatNetworkElements)
            {
                if (hnElement.Name.ToString().ToLower() == Element)
                {
                    foreach (var d in hnElement.Documents)
                    {
                        if (d.Phase.ToString() == "Phase" + Phase.ToString() && d.Stage.ToString() == "Stage" + Stage.ToString())
                        {
                            documents.Add(d);
                        }
                    }
                }
            }
            var assessmentPlanDoc = heatNetworkResponse?.Soa?.JourneyData?.AssessorDocs?
                .Where(d => d.Phase == "Phase" + Phase.ToString())
                .Select(d => new DocumentItem
                {
                    Name = "Assessment plan",
                    DocNames = new List<string> { d.FileName },
                    ChangeUrl = "#"
                })
                .FirstOrDefault();

            var elementList = new List<ElementViewModel>();
            var phasesData = _sessionHelper.GetFromSession<List<PhaseViewModel>>(HttpContext, "PhaseData");

            elementList = phasesData[Phase - 1].Stages[Stage - 1].Elements;

            var model = new DownloadTheDocumentModel()
            {
                Phase = "Phase" + Phase,
                Stage = "Stage" + Stage,
                Element = char.ToUpper(Element[0]) + Element.Substring(1),
                ElementList = elementList,
                Documents = documents,
                    AssessementPlan = assessmentPlanDoc
            };
            _sessionHelper.SaveToSession(HttpContext, "ElementName", model.Element);
            _sessionHelper.SaveToSession(HttpContext, "PhaseNumber", Phase.ToString());
            _sessionHelper.SaveToSession(HttpContext, "StageNumber", Stage.ToString());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadTheDocuments(int Phase)
        {
            this.ShowBackButton("HeatNetworkDetails", "Assessor");
            var model = new UploadAssessmentPlanViewModel
            {
                PhaseNumber = Phase,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { Phase })
            };
            return RedirectToAction("UploadSOC", "Assessor", model);
        }

        public async Task<IActionResult> Download([FromQuery] string stage, string filename, string element)
        {
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var soaProject = await _soaService.GetByHnIdAsync(hnid.ToUpper());
            if (soaProject == null)
            {
                return BadRequest();
            }
            var key = "";
            foreach (var hnElement in soaProject.JourneyData.HeatNetworkElements)
            {
                if (hnElement.Name.ToString().ToLower() == element)
                {
                    foreach (var d in hnElement.Documents)
                    {
                        if (d.FileName.ToLower() == filename && d.Stage.ToString().ToLower() == stage)
                        {
                            key = d.S3Key;
                        }
                    }
                }
            }

            var stream = await _s3UploadService.GetFileAsync(key);
            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream", Path.GetFileName(key));
        }

        [HttpGet]
        public IActionResult UploadSOC([FromQuery] int phase)
        {
            this.ShowBackButton("HeatNetworkDetails", "Assessor");
            var model = new UploadSOCViewModel
            {
                PhaseNumber = phase,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phase })
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUploadSOC(int phase, IFormFile assessorSoc)
        {

            phase = phase + 1;
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            if (assessorSoc == null || assessorSoc.Length == 0)
            {
                ModelState.AddModelError("assessmentPlan", "Please select a file to upload.");
                return View(new UploadAssessmentPlanViewModel
                {
                    PhaseNumber = phase,
                    TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phase })
                });
            }

            var s3Key = await _s3UploadService.UploadFileAsync(assessorSoc, $"soa/{hnId}/{phase}/assessorSOC");

            // Optionally persist metadata or update project state here
            var request = new UpdateDocumentRequest(hnId: hnId.ToUpper(), phase: (SoaPhase)phase, uploadedBy: userId, fileName: assessorSoc.FileName, s3Key: s3Key, documentType: DocumentType.Assessor);
            await _soaService.UpdateDocument(request);

            _logger.LogInformation("Assessor document uploaded for HN ID: {HnId}, Phase: {Phase}, UploadedBy: {UserId}, s3Key: {s3Key}", hnId, phase, userId, s3Key);
            return RedirectToAction("CheckYourAnswers");
        }


        [HttpGet]
        public async Task<IActionResult> CheckYourAnswers()
        {
            // soc - get assessorDocs
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnid.ToUpper());
            var elementName = _sessionHelper.GetFromSession<string>(HttpContext, "ElementName");
            var phaseNumber = int.Parse(_sessionHelper.GetFromSession<string>(HttpContext, "PhaseNumber"));
            var stageNumber = int.Parse(_sessionHelper.GetFromSession<string>(HttpContext, "StageNumber"));
            var assessorsoc = heatNetworkResponse?.Soa?.JourneyData?.AssessorDocs?
                .Where(d => d.Phase == "Phase" + phaseNumber.ToString()) // Updated to use phaseNumber directly
                .Select(d => new DocumentItem
                {
                    Name = "Assessment plan",
                    DocNames = new List<string> { d.FileName },
                    ChangeUrl = "#"
                })
                .FirstOrDefault();
            var model = new AssessorCYAModel()
        {
                ElementName = char.ToUpper(elementName[0]) + elementName.Substring(1),
                PhaseName = "Phase" + phaseNumber,
                StageName = "Stage" + stageNumber,
                SOCfileName = assessorsoc != null && assessorsoc.DocNames.Count > 0 ? assessorsoc.DocNames[0] : "No file uploaded"
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult SOCSubmitted()
        {
            var model = new SOCSubmittedModel { ElementName = _sessionHelper.GetFromSession<string>(HttpContext, "ElementName") };
            return View(model);
        }
    }
}
