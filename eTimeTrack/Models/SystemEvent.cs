using System;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class SystemEvent
    {
        [Key]
        public int SystemEventId { get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        [Display(Name = "Event Title")]
        [Required]
        public string EventTitle{ get; set; }
        [Required]
        public DateTime DateTime { get; set; }
    }
}