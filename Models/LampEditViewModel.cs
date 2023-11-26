using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ActiveLights_MVC.Models
{
    public class LampEditViewModel
    {
        [BindProperty]
        [DisplayName("Lamp ID")]
        public string? LampItemId { get; set; }
        [BindProperty]
        [DisplayName("Name")]
        public string? Name { get; set; }
        [BindProperty]
        [DisplayName("Location")]
        public string? Location { get; set; }
        [BindProperty]
        [DisplayName("Installation Date")]
        [DataType(DataType.DateTime)]
        public DateTime? InstallationDate { get; set; }
        [BindProperty]
        [DisplayName("IP Address")]
        public string? IpAddress { get; set; }
        [BindProperty]
        [DisplayName("Group")]
        public string? LampGroup { get; set; }
        [BindProperty]
        [DisplayName("Neighbors Backward")]
        public string? NeighborsBack { get; set; }
        [BindProperty]
        [DisplayName("Neighbors Forward")]
        public string? NeighborsForward { get; set; }
        [BindProperty]
        [DisplayName("Level High Value")]
        public int LevelHigh { get; set; }
        [BindProperty]
        [DisplayName("Level Low Level")]
        public int LevelLow { get; set; }
        [BindProperty]
        [DisplayName("Level Off")]
        public int LevelOff { get; set; }
        [BindProperty]
        [DisplayName("Time On Evening")]
        public TimeOnly TimeLightOnEvening { get; set; }
        [BindProperty]
        [DisplayName("Time LowLevel Evening")]
        public TimeOnly TimeLightLowNight { get; set; }
        [BindProperty]
        [DisplayName("Time On Morning")]
        public TimeOnly TimeLightOnMorning { get; set; }
        [BindProperty]
        [DisplayName("Time Off Morning")]
        public TimeOnly TimeLightOffMorning { get; set; }
    }
}
