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
        public const string DeedPollViewModelSessionKey = "DeedPollViewModel";
        public const string CheckYourAnswersOrganisationModelSessionKey = "CheckYourAnswersOrganisationModel";

        public const string UserRoleKey = "UserRoleKey";
        public const string OrganisationContactDetailsModelSessionKey = "OrganisationContactDetailsModelSessionKey";


        // Session key for address
        public const string SearchAddressByPostcodeModelSessionKey = "SearchAddressByPostcodeModel";
        public const string AddressByStreetOrTownModelSessionKey = "AddressByStreetOrTownModel";

        // Session key for the boolean flow state
        public const string IsCheckAnswerFlowKey = "IsCheckAnswerFlow";

        // Session keys for Registration eligibility flow
        public const string WhatDoYouWantToDoViewModelKey = "WhatDoYouWantToDoViewModel";
        public const string AreYouTheRPModelKey = "AreYouTheRP";
        public const string IsYourOrgWorkingOnANewHNModelKey = "IsYourOrgWorkingOnANewHN";
        public const string IsHNLocatedInEnglandScotlandWalesModelKey = "IsHNLocatedInEnglandScotlandWales";
        public const string HowManyDwellingsIncludedModelKey = "HowManyDwellingsIncluded";

        // Session keys for heat network registration
        public const string HeatNetworkOrganisationModelKey = "HeatNetworkOrganisationModelKey";
        public const string IsHnTypeCommunalViewModel = "IsHnTypeCommunalViewModelKey";
        public const string DoesCommunalHnHaveOwnEcViewModel = "DoesCommunalHnHaveOwnEcViewModel";
        public const string DoesDistrictHnHaveOwnEcViewModel = "DoesDistrictHnHaveOwnEcViewModel";
        public const string DoesCommunalEcSupplyOneBlockViewModel = "DoesCommunalEcSupplyOneBlockViewModel";
        public const string HeatNetworkConnectionsViewModelKey = "HeatNetworkConnectionsViewModelKey";
        public const string HeatNetworkNameModelKey = "HeatNetworkName";
        public const string DoesHNHaveAPostcodeViewModelKey = "DoesHNHaveAPostcodeViewModel";
        public const string HeatNetworkLocationModelKey = "HNLocation";
        public const string ECDetailsModelSessionKey = "ECDetailsModelSessionKey";
        public const string HeatNetworkPhaseModelKey = "HeatNetworkPhase";
        public const string PathwayModelKey = "Pathway";
        public const string CheckYourAnswersHeatNetworkModelKey = "CheckYourAnswersHeatNetworkModelKey";
        public const string HeatNetworkSuccessRedirectionSessionKey = "HeatNetworkSuccessRedirectionSessionKey";

        public const string HnId = "HnIdkey";
        public const string HnName = "HnNameKey";


        // Session for adding DDH and contributor
        public const string WhoDoYouWantToAddSessionKey = "WhoDoYouWantToAddSessionKey";
        public const string NewContributorRoleViewModelSessionKey = "NewContributorRoleViewModel";
        public const string AddContributorViewModelSessionKey = "AddContributorViewModel";
        public const string NewContributorDetailsViewModelSessionKey = "NewContributorDetailsViewModel";
        public const string ExistingContributorsListViewModelSessionKey = "ExistingContributorsListViewModelSessionKey";
        public const string ContributorsHeatNetworkPhaseViewModelSessionKey = "ContributorsHeatNetworkPhaseViewModelSessionKey";
        public const string NewContributorHeatNetworkViewModelSessionKey = "NewContributorHeatNetworkViewModel";
        public const string CheckYourAnswersContributorsModelSessionKey = "CheckYourAnswersContributorsModelSessionKey";        

        // Session keys for invitations
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
        public const string DoesEnergyCentreHaveAPostcodeViewModelSessionKey = "DoesEnergyCentreHaveAPostcodeViewModel";
        public const string EnergyCentreLocationModelKey = "EnergyCentreLocation";
        public const string EnergyCentreDetailsModelSessionKey = "EnergyCentreDetailsModelSessionKey";
        public const string SelectedElementsSessionKey = "SelectedElements";

        public const string NetworkElementsViewModelSessionKey = "NetworkElementsViewModelSessionKey";
        public const string NetworkDetailsUploadSessionKey = "NetworkDetailsUploadSessionKey";
        public const string ElementSoaViewModelSessionKey = "ElementSoaViewModelSessionKey";
        public const string ElementSoaUploadViewModelSessionKey = "ElementSoaUploadViewModelSessionKey";
        public const string ElementSoaIncompleteSoaSessionKey = "ElementSoaIncompleteSoaSessionKey";
        public const string CurrentStageIndexSessionKey = "CurrentStageIndexSessionKey";
        public const string ElementSoaStatusUpdateModelSessionKey = "ElementSoaStatusUpdateModelSessionKey";       
        
        public const string NetworkElementsOverViewModelSessionKey = "NetworkElementsOverViewModelSessionKey";        
        public const string IsAddOrganisationDetailsRPJourneySessionKey = "IsAddOrganisationDetailsRPJourneySessionKey";
        public const string AssessorSelectElementsViewModelSessionKey = "AssessorSelectElementsViewModelSessionKey";
        public const string AssessorSelectedElementSessionKey = "AssessorSelectedElementSessionKey";
        public const string AssessorAssessmentSelectionViewModelSessionKey = "AssessorAssessmentSelectionViewModelSessionKey";
        public const string AssessorDetailsSessionKey = "AssessorDetailsSessionKey";
        public const string NewLeadDetailsViewModelSessionKey = "NewLeadDetailsViewModelSessionKey";
        public const string AssessorElementSelectionOverviewModelSessionKey = "AssessorElementSelectionOverviewModelSessionKey";
        public const string SoaStageOfAssessorOnboarding = "SoaStageOfAssessorOnboarding";
        public const string DefaultSelectedAssessor = "DefaultSelectedAssessor";
        public const string AssessorSearchResultsSessionKey = "AssessorSearchResultsSessionKey";
        public const string SubstationViewModelKey = "SubstationViewModelKey";
        public const string DistributionNetworksViewModelKey = "DistributionNetworksViewModelKey";

    }
}
