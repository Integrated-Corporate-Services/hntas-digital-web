using System.ComponentModel.DataAnnotations;


namespace HNTAS.Web.UI.Models.HeatNetwork
{
    public class HeatNetworkNameModel
    {
        [Required(ErrorMessage = "Please enter the name of the Heat Network.")]
        public string hnName { get; set; }
    }
}
