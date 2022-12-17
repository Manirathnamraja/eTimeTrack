using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class AECOMUserClassificationCreateViewModel
    {
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        public string Classification { get; set; }
        //[StringLength(511, ErrorMessage = "Maximum length is 511")]
        //[Required]
        //public string Description { get; set; }
        public int ProjectID { get; set; }
        public int AECOMUserClassificationId { get; set; }
    }
}