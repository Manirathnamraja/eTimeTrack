using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public abstract class ReconciliationTemplateBaseViewModel
    {
        [Required]
        [DisplayName("Template Name")]
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Name { get; set; }
        [DisplayName("Employee Number Column")]
        [Required]
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string EmployeeNumberColumn { get; set; }
        [DisplayName("Week Ending Column")]
        [Required]
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string WeekEndingColumn { get; set; }
        [DisplayName("Hours Column")]
        [Required]
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string HoursColumn { get; set; }
        [DisplayName("Identifier Column")]
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string TypeIdentifierColumn { get; set; }
        [DisplayName("Identifier Values")]
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string TypeIdentifierText { get; set; }
        [DisplayName("Daily Hours")]
        public bool DailyDates { get; set; }
    }

    public class ReconciliationTemplateCreateViewModel : ReconciliationTemplateBaseViewModel
    {
        [DisplayName("Company")]
        [Required]
        public int CompanyId { get; set; }
    }

    public class ReconciliationTemplateUpdateViewModel : ReconciliationTemplateBaseViewModel
    {
        [Required]
        public int ReconciliationTemplateId { get; set; }
    }
}