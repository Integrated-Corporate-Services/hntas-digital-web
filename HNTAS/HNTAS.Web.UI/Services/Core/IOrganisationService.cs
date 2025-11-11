namespace HNTAS.Web.UI.Services.Core
{
    public interface IOrganisationService
    {
        Task<bool?> GetOrganisationByDetails(string orgName, string postCode, string country);
    }
}
