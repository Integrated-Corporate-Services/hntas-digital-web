using HNTAS.Web.UI.Models.Common;

namespace HNTAS.Web.UI.Models.Components
{
    public class GovUkRadioListModel
    {
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public string SelectedValue { get; set; }
        public List<SelectItemOption> Items { get; set; }
    }
}
