using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public abstract class ProjectUserTypeBaseViewModel
    {
        public int ProjectID { get; set; }
        [MaxLength(255)]
        [DisplayName("Code Alias")]
        public string AliasCode { get; set; }
        [MaxLength(255)]
        [DisplayName("Type Alias")]
        public string AliasType { get; set; }
        [MaxLength(255)]
        [DisplayName("Description Alias")]
        public string AliasDescription { get; set; }
        [DisplayName("Maximum NT Hours")]
        public float? MaxNTHours { get; set; }
        [DisplayName("Maximum OT1 Hours")]
        public float? MaxOT1Hours { get; set; }
        [DisplayName("Maximum OT2 Hours")]
        public float? MaxOT2Hours { get; set; }
        [DisplayName("Maximum OT3 Hours")]
        public float? MaxOT3Hours { get; set; }
    }

    public class ProjectUserTypeCreateViewModel : ProjectUserTypeBaseViewModel
    {
        [Required]
        [DisplayName("User Type")]
        public int? UserTypeID { get; set; }
    }

    public class ProjectUserTypeUpdateViewModel : ProjectUserTypeBaseViewModel
    {
        [DisplayName("User Type")]
        public int UserTypeID { get; set; }

        public int? ProjectUserTypeID { get; set; }
        [DisplayName("User Type")]
        public string UserTypeName { get; set; }

        public bool IsGenericUserType { get; set; }
    }
}