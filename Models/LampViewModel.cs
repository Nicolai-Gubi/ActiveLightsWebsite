using System.ComponentModel.DataAnnotations;

namespace ActiveLights_MVC.Models
{
    public class LampViewModel
    {
        public string? LampItemId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? TimeLightOnEvening { get; set; }
        public string? TimeLightLowNight { get; set; }
        public string? TimeLightOnMorning { get; set; }
        public string? TimeLightOffMorning { get; set; }
        public string? CurrentLightLevel { get; set; }
        public string? CurrentErrorStatus { get; set; }
    }
}
