using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class ProjectDisciplineCreateViewModel
    {
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Required]
        public string Text { get; set; }

        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Display(Name = "Discipline Type")]
        [Required]
        public string Description { get; set; }
       // public int ProjectID { get; set; }
        public int ProjectDisciplineId { get; set; }
    }
}