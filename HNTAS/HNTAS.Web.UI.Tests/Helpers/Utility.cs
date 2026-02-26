using HNTAS.Api.Client.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;


namespace HNTAS.Web.UI.Tests.Helpers
{
    public static class TestingUtility
    {
        public static Mock<IUrlHelper> SetUpBackLink(string controller, string action)
        {
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == action && ctx.Controller == controller)))
                .Returns($"{controller}/{action}");
            return urlHelperMock;
        }

        public static UserDetailsResponse MockValid_UserService_GetUserDetails(string userId)
        {
            var userDetails = new HNTAS.Api.Client.Model.UserDetailsResponse
            {
                Id = userId,
                OneLoginId = "one-login-id",
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                EmailId = "test@email.com",
                JobTitle = "Assessor",
                MobileNumber = "1234567890",
                Status = UserStatus.Active,
                Roles = new List<UserRole> { UserRole.Assessor },
                Organisation = new OrganisationResponse
                {
                    OrgId = "org-id",
                    Name = "Test Organisation",
                    CompaniesHouseNumber = "12345678",
                    Type = OrganisationType.UkCompaniesHouse,
                    RegisteredAddress = new RegisteredAddress2("123 Test St", "TE1 1ST", "Test Area", "Test Town", "Test County", "Test Country")
                },
                HeatNetworks = new List<HeatNetworkUserResponse>()
                {
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-1",
                        Name = "Heat Network 1",
                        //Location = "Location 1"
                    },
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-2",
                        Name = "Heat Network 2",
                       // Location = "Location 2"
                    }
                }
            };

            return userDetails;
        }

        public static HeatNetworkResponse MockValid_HNService_GetAsync(string hnId)
        {
            return new HeatNetworkResponse
            {
                Id = "heat-network-id",
                HnId = hnId,
                //Location = "///pretty.nice.stuff",
                Name = "Test Network",
                Pathway = "1",
                Soa = new SoaResponse
                {
                    Status = SoaStatus.InProgress,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "user",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "user",
                    JourneyData = new JourneyDataResponse
                    {
                        NetworkType = new NetworkTypeResponse(),
                        ConnectionTypes = new List<string>(),
                        HeatNetworkElements = new List<HeatNetworkElementResponse>()
                        {
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementDisplayType.EnergyCentre.ToString(),
                                Count = 1,
                                Locations = new List<string>{ "Location 1" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "energy_centre_doc.docx",
                                        S3Key = "energy_centre_123",
                                        Phase = "Phase1",
                                        Stage = "Stage1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            },
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementDisplayType.DistributionNetwork.ToString(),
                                Count = 1,
                                Locations = new List<string>{ "Location 2" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "distribution_network_doc.docx",
                                        S3Key = "distribution_network_123",
                                        Phase = "Phase1",
                                        Stage = "Stage1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            },
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementDisplayType.ThermalSubStation.ToString(),
                                Count = 2,
                                Locations = new List<string>{ "Location 3", "Location 4" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "thermal_sub_station_doc1.docx",
                                        S3Key = "thermal_sub_station_123",
                                        Phase = "1",
                                        Stage = "1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    },
                                    new UploadedDocumentResponse(){
                                        FileName = "thermal_sub_station_doc2.docx",
                                        S3Key = "thermal_sub_station_456",
                                        Phase = "1",
                                        Stage = "1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            }
                        },
                        AssessmentDocs = new List<UploadedAssessmentDocumentResponse> {
                            new UploadedAssessmentDocumentResponse(){
                                FileName = "testfile123.docx",
                                S3Key = "test123",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                        AssessorDocs = new List<UploadedAssessorDocumentResponse>() {
                            new UploadedAssessorDocumentResponse(){
                                FileName = "testfile456.docx",
                                S3Key = "test456",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                        CertifierDocs = new List<UploadedCertifierDocumentResponse>() {
                            new UploadedCertifierDocumentResponse(){
                                FileName = "testfile789.docx",
                                S3Key = "test789",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                    }
                },
                Address = new RegisteredAddress
                (
                    "AddressLine1",
                    "Postalcode1",
                    "AddressLine2",
                    "Town1",
                    "County1",
                    "Countrry1"
                )// assuming this is a valid populated object
            };
        }

        public static List<UserResponse> MockValid_UserService_GetRegisteredUsers(string rpUserId)
        {
            return new List<UserResponse>
            {
                new UserResponse
                {
                    Id = "user-1",
                    OneLoginId = "one-login-id-1",
                    FirstName = "Alice",
                    LastName = "Smith",
                    FullName = "Alice Smith",
                    EmailId = "alicesmith@test.com",
                    JobTitle = "Test1",
                    PreferredContactType = NullableOfPreferredContactType.Mobile,
                    MobileNumber = "1112223333",
                    Status = UserStatus.Active,
                    Roles = new List<UserRole>() {
                        UserRole.Assessor
                    },
                    OrgId = "org-1",
                    HnRoleMappings = new List<HnRoleMapping>
                    {
                        new HnRoleMapping
                        {
                            HnId = "hn-1",
                            Role = ContributorRole.Assessor
                        }
                    }
                },
                new UserResponse
                {
                    Id = "user-2",
                    OneLoginId = "one-login-id-2",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    FullName = "Bob Johnson",
                    EmailId = "BobJohnson@test.com",
                    JobTitle = "Test2",
                    PreferredContactType = NullableOfPreferredContactType.Mobile,
                    MobileNumber = "4445556666",
                    Status = UserStatus.Active,
                    Roles = new List<UserRole>() {
                        UserRole.Contractor
                    },
                    OrgId = "org-1",
                    HnRoleMappings = new List<HnRoleMapping>
                    {
                        new HnRoleMapping
                        {
                            HnId = "hn-1",
                            Role = ContributorRole.DesignatedContractor
                        }
                    }
                }
            };
        }

        public static UserResponse MockValid_UserService_GetUserById(string userId)
        {
            return new UserResponse
            {
                Id = "user-1",
                OneLoginId = "one-login-id-1",
                FirstName = "Alice",
                LastName = "Smith",
                FullName = "Alice Smith",
                EmailId = "alicesmith@test.com",
                JobTitle = "Test1",
                PreferredContactType = NullableOfPreferredContactType.Mobile,
                MobileNumber = "1112223333",
                Status = UserStatus.Active,
                Roles = new List<UserRole>()
                {
                    UserRole.Assessor
                },
                OrgId = "org-1",
                HnRoleMappings = new List<HnRoleMapping>
                {
                    new HnRoleMapping
                    {
                        HnId = "hn-1",
                        Role = ContributorRole.Assessor
                    }
                }
            };
        }
    }
}
