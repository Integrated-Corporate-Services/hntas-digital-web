using HNTAS.Web.UI.Models.Common;

namespace HNTAS.Web.UI.Models.Components
{
    public class GovUkCheckboxListModel
    {
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public List<string> SelectedValues { get; set; } = new List<string>();
        public List<SelectItemOption> Items { get; set; }
    }
}
