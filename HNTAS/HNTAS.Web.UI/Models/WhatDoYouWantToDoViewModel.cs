using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class WhatDoYouWantToDoViewModel
    {
        [Required(ErrorMessage = "Select whether you want to register an organisation or sign in")]
        public string UserPathToday { get; set; } = string.Empty;
    }
}
