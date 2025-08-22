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
        public const string HowManyDwellingsIncludedModelKey = "howManyDwellingsIncluded";
        public const string IsHNCurrentlyOperatingModelKey = "isHNCurrentlyOperating";
        public const string HaveYouSignedMEContractModelKey = "haveYouSignedMEContract";

        public const string HeatNetworkLocationModelKey = "HeatNetworkLocation";
        public const string HeatNetworkNameModelKey = "HeatNetworkName";
    }
}
