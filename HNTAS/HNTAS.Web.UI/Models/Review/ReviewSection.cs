namespace HNTAS.Web.UI.Models.Review
{
    public class ReviewSection
    {
        public string Heading { get; set; }
        public List<ReviewItem> Items { get; set; } = new List<ReviewItem>();
    }
}
