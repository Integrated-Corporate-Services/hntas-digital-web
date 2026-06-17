using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class CertifierController : Controller
    {
        private readonly ILogger<CertifierController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IUserService _userService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly ISoaService _soaService;
        private readonly IS3UploadService _s3UploadService;

        public CertifierController(ILogger<CertifierController> logger,
            ISessionHelper sessionHelper,
            IUserService userService,
            IHeatNetworkService heatNetworkService,
            ISoaService soaService,
            IS3UploadService s3UploadService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _userService = userService;
            _heatNetworkService = heatNetworkService;
            _soaService = soaService;
            _s3UploadService = s3UploadService;
        }


        [HttpGet]
        public async Task<IActionResult> HeatNetworkDetails([FromQuery] string hnid)
        {

            this.ShowBackButton("HeatNetworks", "UserManagement");
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnid.ToUpper());
            try
            {
                var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
                var hnDetails = await _heatNetworkService.GetAsync(hnid?.ToUpper());
                

                if (user == null || hnDetails == null || hnDetails.Soa == null)
                {
                    return BadRequest();
                }
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnDetails.Name);
                var phaseIndex = 0;


                var model = new HeatNetworkDetailsViewModel
                {
                    HnId = hnDetails.HnId,
                    HnName = hnDetails.Name,
                    Address = new Models.Address.AddressByStreetOrTownModel
                    {
                        StreetAddress = hnDetails.Address.AddressLine1,
                        TownOrCity = hnDetails.Address.Town,
                        Postalcode = hnDetails.Address.Postcode,
                        Country = hnDetails.Address.Country
                    },
                    OrganisationName = user.Organisation.Name,
                    OrganisationAddress = new RegisteredAddress(
                        user.Organisation?.RegisteredAddress?.AddressLine1 ?? string.Empty,
                        user.Organisation?.RegisteredAddress?.AddressLine2 ?? string.Empty,
                        user.Organisation?.RegisteredAddress?.Town ?? string.Empty,
                        user.Organisation?.RegisteredAddress?.County ?? string.Empty,
                        user.Organisation?.RegisteredAddress?.Postcode ?? string.Empty,
                        user.Organisation?.RegisteredAddress?.Country ?? string.Empty
                    ),
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
        public async Task<IActionResult> DownloadTheDocuments(int phase)
        {
            this.ShowBackButton("HeatNetworkDetails", "Certifier");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnId);

            ViewBag.HeatNetworkName = heatNetworkResponse?.Name;

            var elementItems = new List<ElementItem>();
            var elementDocuments = new List<DocumentItem>();
            if (heatNetworkResponse == null || heatNetworkResponse?.Soa?.JourneyData?.HeatNetworkElements == null)
            {
                _logger.LogWarning("Could not retrieve Heat Network details for ID: {HnId}", hnId);
                ModelState.AddModelError("HeatNetworkDetailsNotFound", "We couldn't retrieve the Heat Network details for the provided ID. Please try again or contact support.");
                return View(phase);
            }


            foreach (var element in heatNetworkResponse?.Soa?.JourneyData?.HeatNetworkElements)
            {
                var elementItem = new ElementItem
                {
                    Name = Utility.GetElementOptions()?.FirstOrDefault(x => x.Id.ToString().ToLower() == element.Name.ToLower())?.Label,
                    Count = element.Count ?? 0,
                };

                elementItems.Add(elementItem);

                var elementItemDocs = new DocumentItem
                {
                    Name = Utility.GetElementOptions()?.FirstOrDefault(x => x.Id.ToString().ToLower() == element.Name.ToLower())?.Label,
                    Documents = element.Documents?.Select(d => new DocumentReference { FileName = d.FileName, DownloadUrl = Url.Action("DownloadElementFile", new { phase = d.Phase, stage = d.Stage, filename = d.FileName, element = element.Name }) }).ToList() ?? new List<DocumentReference>(),
                    ChangeUrl = "#"
                };

                elementDocuments.Add(elementItemDocs);
            }

            //filter assessment plan for current phase
            var assessmentPlanDoc = heatNetworkResponse?.Soa?.JourneyData?.AssessmentDocs?
                .Where(d => d.Phase == "Phase" + phase)
                .Select(d => new DocumentItem
                {
                    Name = "Assessment plan",
                    Documents = new List<DocumentReference>
                                {
                                    new DocumentReference
                                    {
                                        FileName = d.FileName,
                                        DownloadUrl = Url.Action("DownloadAssessmentFile")
                                    }
                                },
                    ChangeUrl = "#"
                })
                .FirstOrDefault();

            //filter assessment plan for current phase
            var assessorDoc = heatNetworkResponse?.Soa?.JourneyData?.AssessorDocs?
                .Where(d => d.Phase == "Phase" + phase)
                .Select(d => new DocumentItem
                {
                    Name = "Assessor Doc",
                    Documents = new List<DocumentReference>
                                {
                                    new DocumentReference
                                    {
                                        FileName = d.FileName,
                                        DownloadUrl = Url.Action("DownloadAssessorFile")
                                    }
                                },
                    ChangeUrl = "#"
                })
                .FirstOrDefault();


            var soaSummaryModel = new SOAReviewSummaryViewModel
            {
                Phase = phase.ToString(),

                Elements = elementItems,

                ElementDocuments = elementDocuments,

                AssessmentPlanDocument = assessmentPlanDoc,

                AssessorDocument = assessorDoc
            };

            return View(soaSummaryModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDownloadTheDocuments(int Phase)
        {
            this.ShowBackButton("HeatNetworkDetails", "Certifier");
            var model = new UploadAssessmentPlanViewModel
            {
                PhaseNumber = Phase,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { Phase })
            };
            return RedirectToAction("UploadCertificate", "Certifier", model);
        }

        public async Task<IActionResult> DownloadElementFile([FromQuery] string phase, string stage, string filename, string element)
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
                        if (d.FileName.ToLower() == filename && d.Phase.ToString().ToLower() == phase.ToLower() && d.Stage.ToString().ToLower() == stage.ToLower())
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

        public async Task<IActionResult> DownloadAssessmentFile()
        {
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnDetails = await _heatNetworkService.GetAsync(hnid.ToUpper());
            if (hnDetails == null)
            {
                return BadRequest();
            }
            var key = hnDetails.Soa.JourneyData.AssessmentDocs.FirstOrDefault()?.S3Key;

            var stream = await _s3UploadService.GetFileAsync(key);
            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream", Path.GetFileName(key));
        }

        public async Task<IActionResult> DownloadAssessorFile()
        {
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnDetails = await _heatNetworkService.GetAsync(hnid.ToUpper());
            if (hnDetails == null)
            {
                return BadRequest();
            }
            var key = hnDetails.Soa.JourneyData.AssessorDocs.FirstOrDefault()?.S3Key;

            var stream = await _s3UploadService.GetFileAsync(key);
            if (stream == null)
                return NotFound();

            return File(stream, "application/octet-stream", Path.GetFileName(key));
        }

        [HttpGet]
        public IActionResult UploadCertificate([FromQuery] int phase)
        {
            this.ShowBackButton("DownloadTheDocuments", "Certifier", new { phase });
            ViewBag.HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var model = new UploadSOCViewModel
            {
                PhaseNumber = phase,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phase })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUploadCertificateAsync(int phase, IFormFile certifierSoc)
        {
            phase = phase + 1;
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            if (certifierSoc == null || certifierSoc.Length == 0)
            {
                this.ShowBackButton("DownloadTheDocuments", "Certifier", new { phase });
                ModelState.Clear();
                ModelState.AddModelError("certifier", "Please select a file to upload.");
                ViewBag.HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
                return View("UploadCertificate", new UploadSOCViewModel
                {
                    PhaseNumber = phase,
                    TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { phase })
                });
            }

            var s3Key = await _s3UploadService.UploadFileAsync(certifierSoc, $"soa/{hnId}/{phase}/certifierSOC");

            // Optionally persist metadata or update project state here
            var request = new UpdateDocumentRequest(hnId: hnId.ToUpper(), phase: (SoaPhase)phase, uploadedBy: userId, fileName: certifierSoc.FileName, s3Key: s3Key, documentType: DocumentType.Certifier);
            await _soaService.UpdateDocument(request);

            _logger.LogInformation("certifier document uploaded for HN ID: {HnId}, Phase: {Phase}, UploadedBy: {UserId}, s3Key: {s3Key}", hnId, phase, userId, s3Key);
            return RedirectToAction("Declaration");
        }

        [HttpGet]
        public IActionResult Declaration()
        {
            this.ShowBackButton("UploadCertificate");
            ViewBag.HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var model = new CertifierConfirmationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDeclarationAsync(CertifierConfirmationViewModel model)
        {
            if (!model.IsConfirmed)
            {
                ModelState.AddModelError("", "You must confirm and accept the declaration to continue.");
                ViewBag.HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
                return View("Declaration", model);
            }


            await _soaService.SendCertificationCompleteEmail(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName), _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId));

            return RedirectToAction("Confirmation");
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            ViewBag.HeatNetworkName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            return View();
        }


        private List<ElementViewModel> GetElementsForStage(NullableOfSoaStage stage, int phaseNumber, SoaResponse soa)
        {
            var phaseEnum = (SoaPhase)phaseNumber;

            var networkElements = new List<ElementViewModel>();

            foreach (var element in soa.JourneyData.HeatNetworkElements)
            {
                var label = Utility.GetElementOptions()?.FirstOrDefault(e => e.Id.ToString() == element.Name)?.Label ?? string.Empty;

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
                    Name = label,
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
    }
}
