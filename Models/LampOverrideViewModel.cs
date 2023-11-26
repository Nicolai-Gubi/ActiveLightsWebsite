using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ActiveLights_MVC.Models
{
    public class LampOverrideViewModel
    {
        [BindProperty]
        public string? LampItemId { get; set; }
        [BindProperty]
        public int Level { get; set; }
    }
}
