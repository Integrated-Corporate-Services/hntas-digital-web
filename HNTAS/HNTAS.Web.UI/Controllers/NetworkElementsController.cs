using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using Element = HNTAS.Web.UI.Models.NetworkElements.Element;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class NetworkElementsController : Controller
    {
        private readonly ILogger<NetworkElementsController> _logger;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IUserService _userService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;

        public NetworkElementsController(ILogger<NetworkElementsController> logger, IHeatNetworkService heatNetworkService, IUserService userService, ISessionHelper sessionHelper, IOrganisationService organisationService, IAddressLookupService addressLookupService)
        {
            _logger = logger;
            _heatNetworkService = heatNetworkService;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public async Task<IActionResult> SelectNetworkElementsAsync([FromQuery] string hnId)
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            hnId = hnId ?? _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            ViewBag.HnId = hnId.ToUpper();
            var model = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);

            if (model != null)
            {
                return View(model);
            }
            model = new NetworkElementViewModel()
            {
                ElementOptions = Utility.GetDefaultNetworkElementOptions()
            };
            
            //var networkType = _sessionHelper.GetFromSession<HeatNetworkType>(HttpContext, SessionKeys.HeatNetworkTypeSessionKey);

            //if (networkType == HeatNetworkType.CommunalHeatNetwork)
            //{                 
            //    //model.ElementOptions = model.ElementOptions
            //    //    .Where(e => e.Id == HeatNetworkElementDisplayType.EnergyCentre || e.Id == HeatNetworkElementDisplayType.ConsumerHeatSystems)
            //    //    .ToList();
            //}

            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var selectedNetworkElements = heatNetworkData?.NetworkElements;

            if (selectedNetworkElements != null)
            {
                selectedNetworkElements.Elements?.ForEach(e =>
                {
                    var displayType = Utility.GetNetworkElementDisplayTypeById(e.ElementType!);
                    model.SelectedElementIds.Add(displayType);
                    if (e.Count.HasValue)
                    {
                        model.ElementCounts[displayType] = e.Count.Value;
                    }
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectNetworkElements(NetworkElementViewModel model)
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            model.ElementOptions = Utility.GetDefaultNetworkElementOptions();

            //var networkType = _sessionHelper.GetFromSession<HeatNetworkType>(HttpContext, SessionKeys.HeatNetworkTypeSessionKey);

            //if (networkType == HeatNetworkType.CommunalHeatNetwork)
            //{
            //    //model.ElementOptions = model.ElementOptions
            //    //    .Where(e => e.Id == HeatNetworkElementDisplayType.EnergyCentre || e.Id == HeatNetworkElementDisplayType.ConsumerHeatSystems)
            //    //    .ToList();
            //}

            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            var elements = new List<Element>();

            foreach (var selectedId in model.SelectedElementIds)
            {
                var ele = new Element
                {
                    Type = selectedId,
                    Count = model.ElementCounts.TryGetValue(selectedId, out var cnt) ? cnt : null
                };
                elements.Add(ele);
                if ((selectedId != HeatNetworkElementDisplayType.EnergyCentre) && (!model.ElementCounts.TryGetValue(selectedId, out var count) || count == null || count <= 0))
                {
                    var element = Utility.GetDefaultNetworkElementOptions().FirstOrDefault(x => x.Id == selectedId);
                    if (element == null)
                    {
                        return BadRequest();
                    }
                    ModelState.AddModelError($"ElementCounts[{selectedId}]", $"Enter number of {element.Label}.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SelectedElementsSessionKey, elements);
            
            await SaveNetworkElements(new NetworkElementsModel());
            ClearNetworkElementSpecificSession();
            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        private async Task SaveNetworkElements(NetworkElementsModel viewModel)
        {            
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            var elements = _sessionHelper.GetFromSession<List<Element>>(HttpContext, SessionKeys.SelectedElementsSessionKey);

            var request = new NetworkElements2
            {
                Elements = elements?.Select(e => new Api.Client.Model.Element(type: e.Type, count: e.Count)).ToList(),
                NetworkElementStatus = NetworkDetailsStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                ElementSoaStatus = NetworkDetailsStatus.ReadyToStart
            };            

            if (request.Elements != null)
            {
                request.Elements = request.Elements.Select((element, index) =>
                {                    
                    element.ElementId = (index + 1).ToString("D5");
                    element.ElementType = Utility.GetNetworkElementIdByType(element.Type.ToString()!);
                    return element;
                }).ToList();
            }
            await _heatNetworkService.UpdateNetworkElements(hnId!, request);
        }

        private void ClearNetworkElementSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SelectedElementsSessionKey);
        }
    }
}
