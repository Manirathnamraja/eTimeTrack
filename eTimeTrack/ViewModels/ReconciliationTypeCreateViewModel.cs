using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class ReconciliationTypeCreateViewModel
    {
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        public string Text { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        public string Description { get; set; }
    }
}