using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class WhatDoYouWantToDoViewModel
    {
        [Required(ErrorMessage = "Please select an option.")]
        public string UserPathToday { get; set; } = string.Empty;
    }
}
