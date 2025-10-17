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
        private readonly IInvitationService _invitationService;
        private readonly IInvitationTokenService _iInvitationTokenService;
        private readonly CertifierEmailGeneratorService _certifierEmailGeneratorService;


        public AssessorController(ILogger<AssessorController> logger,
            ISessionHelper sessionHelper,
            IUserService userService,
            IHeatNetworkService heatNetworkService,
            ISoaService soaProjectService,
            IS3UploadService s3UploadService,
            IInvitationService invitationService,
            IInvitationTokenService iInvitationTokenService,
            CertifierEmailGeneratorService certifierEmailGeneratorService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _userService = userService;
            _heatNetworkService = heatNetworkService;
            _soaService = soaProjectService;
            _s3UploadService = s3UploadService;
            _invitationService = invitationService;
            _iInvitationTokenService = iInvitationTokenService;
            _certifierEmailGeneratorService = certifierEmailGeneratorService;
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
            if (_sessionHelper.GetFromSession<string>(HttpContext, "HasDeclaredImpartiality") == "true")
            {
                return RedirectToAction("HeatNetworkDetails", "Assessor", new { hnId = hnid });
            }
            this.ShowBackButton("HeatNetworks", "UserManagement");
            var model = _sessionHelper.GetFromSession<DeclationOfImpartialityModel>(HttpContext, SessionKeys.DeclarationOfImpartialityModelKey) ?? new DeclationOfImpartialityModel();
            model.HnId = hnid;
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
            _sessionHelper.SaveToSession(HttpContext, "HasDeclaredImpartiality", "true"); // Save to db, but where? once per element or once per user or hnid?
            return RedirectToAction("HeatNetworkDetails", "Assessor", new { hnId = model.HnId });
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

        [HttpGet]
        public async Task<IActionResult> HeatNetworkDetails([FromQuery] string hnid)
        {

            this.ShowBackButton("HeatNetworks", "UserManagement");
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnid.ToUpper());

            try
            {
                var user = await _userService.GetUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
                var hnDetails = await _heatNetworkService.GetAsync(hnid?.ToUpper());

                _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnDetails.Name);

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
        public async Task<IActionResult> DownloadTheDocuments(int phase)
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("HeatNetworkDetails", "Assessor", new { hnId });


            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnId);

            ViewBag.HeatNetworkName = heatNetworkResponse?.Name;

            var elementItems = new List<ElementItem>();
            var elementDocuments = new List<DocumentItem>();
            var assessmentPlanDocument = new DocumentItem();


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


            var soaSummaryModel = new SOAReviewSummaryViewModel
            {
                Phase = phase.ToString(),

                Elements = elementItems,

                ElementDocuments = elementDocuments,

                AssessmentPlanDocument = assessmentPlanDoc
            };

            return View(soaSummaryModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDownloadTheDocuments(int Phase)
        {
            this.ShowBackButton("HeatNetworkDetails", "Assessor");
            var model = new UploadAssessmentPlanViewModel
            {
                PhaseNumber = Phase,
                TemplateDownloadUrl = Url.Action("DownloadTemplate", "Soa", new { Phase })
            };
            return RedirectToAction("UploadSOC", "Assessor", model);
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
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> CheckYourAnswersAsync()
        {
            var hnid = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnDetails = await _heatNetworkService.GetAsync(hnid.ToUpper());
            if (hnDetails == null)
            {
                return BadRequest();
            }
            ViewBag.SOCDocFileName = hnDetails.Soa.JourneyData.AssessorDocs.FirstOrDefault()?.FileName;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SOCSubmittedAsync()
        {

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            ViewBag.HnId = hnId;
            try
            {
                var email = _certifierEmailGeneratorService.GenerateCertifierEmail();
                var invitationId = await _invitationService.AddInvitedUserAsync(
                       userId,
                       new AddInvitationRequest(
                           emailAddress: email,
                           firstName: "Certifier",
                           lastName: "Agent",
                           preferredContactType: PreferredContactType.Landline,
                           hnId: hnId,
                           contributorRoles: new List<ContributorRole> { ContributorRole.Certifier },
                           status: InvitationStatus.Invited,
                           landlineNumber: "24723842378",
                           mobileNumber: null,
                           contactNumberExtension: null
                       )
                   );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("CheckYourAnswers");
                }

                _logger.LogInformation("Successfully submitted new certifier details.");
                var token = _iInvitationTokenService.GenerateToken(invitationId, email);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));

                await _soaService.SendAssessorAssessmentEmail(hnName, hnId, "Pass");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new certifier details.");
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }

            return View();
        }
    }
}
