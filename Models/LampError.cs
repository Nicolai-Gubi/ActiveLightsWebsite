namespace ActiveLights_MVC.Models
{
    public class LampError
    {
        public int ErrorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LampId { get; set; }
        public bool ErrorPower { get; set; }
        public bool Errorlamp { get; set; }

    }
}
