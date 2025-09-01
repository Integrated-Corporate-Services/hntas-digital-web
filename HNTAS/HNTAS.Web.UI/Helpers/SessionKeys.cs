namespace HNTAS.Web.UI.Helpers
{
    public static class SessionKeys
    {
        public const string UserCreation_SessionKey = "UserModelDataKey";
        public const string UserModel_Id_SessionKey = "UserModelIdDataKey";
        public const string OrganisationCreation_SessionKey = "OrganisationModelDataKey";
        public const string OrganisationName = "OrganisationNameKey";
        public const string OrganisationId = "OrganisationIdKey";

        // Session key for the boolean flow state
        public const string IsCheckAnswerFlowKey = "IsCheckAnswerFlow";

        // Session keys for specific models
        public const string WhatDoYouWantToDoViewModelKey = "WhatDoYouWantToDoViewModel";
        public const string WhereIsTheHeatNetworkModelKey = "whereIsTheHeatNetwork";        
        public const string IsHNCurrentlyOperatingModelKey = "isHNCurrentlyOperating";
        public const string HaveYouSignedMEContractModelKey = "haveYouSignedMEContract";

        public const string AreYouTheRPModelKey = "AreYouTheRP";
        public const string IsYourOrgWorkingOnANewHNModelKey = "IsYourOrgWorkingOnANewHN";
        public const string IsHNLocatedInEnglandScotlandWalesModelKey = "IsHNLocatedInEnglandScotlandWales";
        public const string HowManyDwellingsIncludedModelKey = "HowManyDwellingsIncluded";


        public const string HeatNetworkNameModelKey = "HeatNetworkName";
        public const string HeatNetworkLocationModelKey = "HeatNetworkLocation";
        public const string HeatNetworkPhaseModelKey = "HeatNetworkPhase";
        public const string HasElementBeenRegisteredModelKey = "HasElementBeenRegistered";
        public const string HasPlanningApplicationBeenSubmittedModelKey = "HasPlanningApplicationBeenSubmitted";
        public const string PathwayModelKey = "Pathway";

        public const string YouHaveBeenInvitedModelKey = "YouHaveBeenInvited";

        public const string InvitedTokenEmail = "InvitedTokenEmailKey";
        public const string InvitationId = "InvitationIdKey";
        public const string InvitedInviterUserId = "InvitedInviterUserIdKey";
        public const string InvitedInviterUserOrgId = "InvitedInviterUserOrgIdKey";

    }
}
