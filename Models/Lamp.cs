using System.ComponentModel.DataAnnotations;

namespace ActiveLights_MVC.Models
{
    public class Lamp
    {
        public string? LampItemId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? InstallationDate { get; set; }
        public string? LampGroup { get; set; }
        public string? NeighborsBack { get; set; }
        public string? NeighborsForward { get; set; }
        public int LevelHigh { get; set; }
        public int LevelLow { get; set; }
        public int LevelOff { get; set; }
        public int LevelOverride { get; set; }
        public int TimeLightOnEvening { get; set; }
        public int TimeLightLowNight { get; set; }
        public int TimeLightOnMorning { get; set; }
        public int TimeLightOffMorning { get; set; }
        public string? IpAddress { get; set; }
    }
}
