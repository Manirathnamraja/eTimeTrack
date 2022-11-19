using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class UserTypeCreateViewModel
    {

        [MaxLength(255)]
        [Required]
        public string Code { get; set; }

        [MaxLength(255)]
        [Required]
        public string Type { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Description { get; set; }
    }
}