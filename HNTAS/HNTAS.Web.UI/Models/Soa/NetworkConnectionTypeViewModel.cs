using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.CustomValidation;
using HNTAS.Web.UI.Models.Common;

namespace HNTAS.Web.UI.Models.Soa
{
    public class NetworkConnectionTypeViewModel
    {
        public List<SelectItemOption> ConnectionTypes { get; set; } = new List<SelectItemOption>();

        [MustHaveOneItem(ErrorMessage = "Select what connection types does your network have.")]
        public List<ConnectionType>? SelectedConnections { get; set; }
    }
}
