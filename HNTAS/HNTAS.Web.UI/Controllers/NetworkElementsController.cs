using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Element = HNTAS.Web.UI.Models.NetworkElements.Element;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetworkDetail)]
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
        public async Task<IActionResult> SelectNetworkElementsAsync()
        {
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);

            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;
            var model = _sessionHelper.GetFromSession<NetworkElementViewModel>(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);

            if (model != null)
            {
                return View(model);
            }

            model = new NetworkElementViewModel();
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            model.ElementOptions = NetworkElementHelper.GetNetworkElementOptionsForNetworkType(networkType);
            ViewBag.Heading = NetworkElementHelper.GetNetworkElementHeadingForNetworkType(networkType);


            var selectedNetworkElements = heatNetworkData?.NetworkElements;

            if (selectedNetworkElements != null)
            {
                selectedNetworkElements.Elements?.ForEach(e =>
                {
                    var displayType = NetworkElementHelper.GetNetworkElementDisplayTypeById(e.ElementType!);
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
            this.ShowBackButton("NetworkDetails", "HeatNetwork");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnId = hnId?.ToUpper();
            ViewBag.HnName = hnName;


            if (!ModelState.IsValid)
            {
                return View("SelectNetworkElements", model);
            }
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var networkType = heatNetworkData?.HeatNetworkType;
            model.ElementOptions = NetworkElementHelper.GetNetworkElementOptionsForNetworkType(networkType);
            ViewBag.Heading = NetworkElementHelper.GetNetworkElementHeadingForNetworkType(networkType);
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
                    var element = NetworkElementHelper.GetNetworkElementOptionsForNetworkType().FirstOrDefault(x => x.Id == selectedId);
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


            var address = heatNetworkData?.Address;
            var coordinates = heatNetworkData?.EcDetails;
            var phase = heatNetworkData?.Phase;

            var latlong = coordinates != null ? $"{coordinates.Latitude},{coordinates.Longitude}" : null;
            var addressByStreetOrTownModel = address != null ? (AddressByStreetOrTownModel)address! : null;

            var networkElementsOverViewModel = new NetworkElementsOverViewModel
            {
                Elements = elements.Select(e =>
                {
                    var elementOption = NetworkElementHelper.GetNetworkElementOptionsForNetworkType().FirstOrDefault(x => x.Id == e.Type);
                    var label = elementOption != null ? elementOption.Label : e.Type.ToString();
                    label = label.ToSentenceCase();

                    return e.Count.HasValue ? $"{e.Count.Value} {label}(s)" : label;
                }).ToList(),
                HeatNetworkAddress = addressByStreetOrTownModel?.Fulladdress,
                Coordinates = latlong,
                NetworkType = "",
                Phase = phase ?? string.Empty
            };

            _sessionHelper.SaveToSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey, networkElementsOverViewModel);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.SelectedElementsSessionKey, elements);

            return RedirectToAction("NetworkElementsOverView", "NetworkElements");
        }

        [HttpGet]
        public IActionResult NetworkElementsOverView()
        {
            this.ShowBackButton("SelectNetworkElements", "NetworkElements");
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId?.ToUpper();
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.HnName = hnName;
            var model = _sessionHelper.GetFromSession<NetworkElementsOverViewModel>(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
            return View("NetworkElementsOverView", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkElementsOverViewAsync()
        {
            await SaveNetworkElements();
            ClearNetworkElementSpecificSession();
            return RedirectToAction("NetworkDetails", "HeatNetwork");
        }

        private async Task SaveNetworkElements()
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
                    element.ElementType = NetworkElementHelper.GetNetworkElementIdByType(element.Type.ToString()!);
                    return element;
                }).ToList();
            }
            await _heatNetworkService.UpdateNetworkElements(hnId!, request);
        }

        private void ClearNetworkElementSpecificSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SelectedElementsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsOverViewModelSessionKey);
        }
    }
}
