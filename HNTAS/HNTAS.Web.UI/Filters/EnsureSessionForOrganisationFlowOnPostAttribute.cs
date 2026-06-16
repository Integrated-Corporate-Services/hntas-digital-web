using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNTAS.Web.UI.Filters
{
    public class EnsureSessionForOrganisationFlowOnPostAttribute : ActionFilterAttribute
    {
        private readonly ISessionHelper _sessionHelper;

        public EnsureSessionForOrganisationFlowOnPostAttribute(ISessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            var organisationModel = _sessionHelper.GetFromSession<OrganisationModel>(context.HttpContext, SessionKeys.OrganisationCreation_SessionKey);

            bool shouldRedirect = false;

            if (controllerName == "Organisation")
            {
                if (organisationModel == null)
                {
                    shouldRedirect = true;
                }
            }
            else if (controllerName == "User")
            {
                var userModel = _sessionHelper.GetFromSession<UserModel>(context.HttpContext, SessionKeys.UserCreation_SessionKey);
                if (organisationModel == null || userModel == null)
                {
                    shouldRedirect = true;
                }
            }


            if (shouldRedirect)
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["ErrorMessage"] = "Your previous session has expired. Please start the process again.";
                }

                context.Result = new RedirectToActionResult("Start", "Organisation", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
