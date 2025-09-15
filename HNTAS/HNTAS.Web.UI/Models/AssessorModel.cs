using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class DeclationOfImpartialityModel
    {
        [Required(ErrorMessage = "You must declare your impartiality in order to progress further.")]
        public bool HasDeclaredImpartiality { get; set; }
        [Required]
        public string HnId { get; set; }
    }
}
