using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ActiveLights_MVC.Models
{
    public class LampStatus
    {
        public long StatusId { get; set; }
        [DisplayName("Lamp")]
        public string? LampId { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayName("Status Data/Time")]
        public DateTime CreateTime { get; set; }
        [DisplayName("Lamp Light Level")]
        public int LampLevel { get; set; }
    }
}