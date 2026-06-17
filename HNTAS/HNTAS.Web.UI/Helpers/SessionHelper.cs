using HNTAS.Web.UI.Extensions;
using Newtonsoft.Json;

namespace HNTAS.Web.UI.Helpers
{
    public class SessionHelper : ISessionHelper
    {
        #region Generic Session Methods

        public void SaveToSession<T>(HttpContext httpContext, string sessionKey, T model)
        {
            if (model == null)
            {
                httpContext.Session.Remove(sessionKey);
                return;
            }
            string json = JsonConvert.SerializeObject(model);
            httpContext.Session.SetString(sessionKey, json);
        }

        public T? GetFromSession<T>(HttpContext httpContext, string sessionKey)
        {
            string? json = httpContext.Session.GetString(sessionKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    // Optionally log the error
                    httpContext.Session.Remove(sessionKey);
                    return default;
                }
            }
            return default;
        }

        public void ClearFromSession(HttpContext httpContext, string sessionKey)
        {
            httpContext.Session.Remove(sessionKey);
        }

        // You might still want a general ClearAllFlowRelatedSessionData if starting completely fresh
        public void ClearAllFlowRelatedSessionData(HttpContext context)
        {
            ClearFromSession(context, SessionKeys.WhatDoYouWantToDoViewModelKey);
            ClearFromSession(context, SessionKeys.UserCreation_SessionKey);
            ClearFromSession(context, SessionKeys.OrganisationCreation_SessionKey);
            ClearFromSession(context, SessionKeys.AddressByStreetOrTownModelSessionKey);
            ClearAllHNRegistrationFlowRelatedSessionData(context);
            context.Session.Remove(SessionKeys.IsCheckAnswerFlowKey);
        }

        public void ClearAllHNRegistrationFlowRelatedSessionData(HttpContext context)
        {
            ClearFromSession(context, SessionKeys.HowManyDwellingsIncludedModelKey);
            ClearFromSession(context, SessionKeys.HeatNetworkOrganisationModelKey);
            ClearFromSession(context, SessionKeys.IsHnTypeCommunalViewModel);
            ClearFromSession(context, SessionKeys.DoesCommunalHnHaveOwnEcViewModel);
            ClearFromSession(context, SessionKeys.DoesDistrictHnHaveOwnEcViewModel);
            ClearFromSession(context, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel);
            ClearFromSession(context, SessionKeys.HeatNetworkConnectionsViewModelKey);
            ClearFromSession(context, SessionKeys.HeatNetworkNameModelKey);
            ClearFromSession(context, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
            ClearFromSession(context, SessionKeys.HeatNetworkLocationModelKey);
            ClearFromSession(context, SessionKeys.ECDetailsModelSessionKey);
            ClearFromSession(context, SessionKeys.HeatNetworkPhaseModelKey);
            ClearFromSession(context, SessionKeys.PathwayModelKey);
            ClearFromSession(context, SessionKeys.CheckYourAnswersHeatNetworkModelKey);
        }

        public void ClearAllContributoFlowRelatedSessionData(HttpContext context) 
        {
            ClearFromSession(context, SessionKeys.NewContributorRoleViewModelSessionKey);
            ClearFromSession(context, SessionKeys.AddContributorViewModelSessionKey);
            ClearFromSession(context, SessionKeys.NewContributorDetailsViewModelSessionKey);
            ClearFromSession(context, SessionKeys.ExistingContributorsListViewModelSessionKey);
            ClearFromSession(context, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            ClearFromSession(context, SessionKeys.CheckYourAnswersContributorsModelSessionKey);
        }

        #endregion

        #region Flow State Methods

        public void SetIsCheckAnswerFlow(HttpContext httpContext, bool isCheckAnswerFlow)
        {
            httpContext.Session.SetBoolean(SessionKeys.IsCheckAnswerFlowKey, isCheckAnswerFlow);
        }

        public bool GetIsCheckAnswerFlow(HttpContext httpContext)
        {
            return httpContext.Session.GetBoolean(SessionKeys.IsCheckAnswerFlowKey) ?? false;
        }

        #endregion
    }
}