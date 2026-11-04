namespace HNTAS.Web.UI.Models.Contributors
{
    public class ManageContributorsViewModel
    {
        public List<ContributorItemDto> Contributors { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Sorting properties
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
    }

    public class ContributorItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OrganisationName { get; set; } = string.Empty;
    }
}
