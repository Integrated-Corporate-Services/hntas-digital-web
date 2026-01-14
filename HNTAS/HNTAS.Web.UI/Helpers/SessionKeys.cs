namespace HNTAS.Web.UI.Helpers
{
    public static class SessionKeys
    {
        public const string UserCreation_SessionKey = "UserModelDataKey";
        public const string UserModel_Id_SessionKey = "UserModelIdDataKey";
        public const string OrganisationCreation_SessionKey = "OrganisationModelDataKey";
        public const string OrganisationName = "OrganisationNameKey";
        public const string OrganisationId = "OrganisationIdKey";
        public const string OrganisationAddress = "OrganisationAddressKey";
        public const string SearchAddressByPostcodeModelSessionKey = "SearchAddressByPostcodeModel";
        public const string AddressByStreetOrTownModelSessionKey = "AddressByStreetOrTownModel";
        public const string UserRoleKey = "UserRoleKey";

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


        public const string HnId = "HnIdkey";
        public const string HnName = "HnNameKey";

        public const string HeatNetworkNameModelKey = "HeatNetworkName";
        public const string HeatNetworkLocationModelKey = "HNLocationByWhat3Words";
        public const string HeatNetworkPhaseModelKey = "HeatNetworkPhase";
        public const string HasElementBeenRegisteredModelKey = "HasElementBeenRegistered";
        public const string HasPlanningApplicationBeenSubmittedModelKey = "HasPlanningApplicationBeenSubmitted";
        public const string PathwayModelKey = "Pathway";

        public const string YouHaveBeenInvitedModelKey = "YouHaveBeenInvited";

        public const string InvitedTokenEmail = "InvitedTokenEmailKey";
        public const string InvitationId = "InvitationIdKey";
        public const string InvitedInviterUserId = "InvitedInviterUserIdKey";
        public const string InvitedInviterUserOrgId = "InvitedInviterUserOrgIdKey";

        public const string SoaProjectId = "SoaProjectIdKey";

        public const string DeclarationOfImpartialityModelKey = "DeclarationOfImpartialityModelKey";

        public const string IsEditOrganisationDetailsJourneySessionKey = "IsEditOrganisationDetailsJourney";
        public const string IsAddOrganisationDetailsNonRPJourneySessionKey = "IsAddOrganisationDetailsNonRPJourneySession";
        public const string IsAssessorOrCertifier = "IsAssessorOrCertifier";

    }
}
