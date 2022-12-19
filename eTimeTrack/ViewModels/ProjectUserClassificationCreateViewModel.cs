using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class ProjectUserClassificationCreateViewModel
    {
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        [DisplayName("Project User Classification Text")]
        public string ProjectClassificationText { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        public string Description { get; set; }
        public int ProjectID { get; set; }
        [DisplayName("AECOM User Classification")]
        public string AECOMUserClassificationID { get; set; }
        public int ProjectUserClassificationId { get; set; }
    }
}