using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectVariation : ITrackableModel, IUserModified, IMergeable
    {
        [Key]
        public int VariationID { get; set; }
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        [StringLength(20, ErrorMessage = "Maximum length is 20"), Required]
        [Display(Name = "Variation Number")]
        public string VariationNo { get; set; }
        [StringLength(5, ErrorMessage = "Maximum length is 5")]
        [Display(Name = "Revision Number")]
        public string RevNo { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
        public DateTime? DateSubmitted { get; set; }
        [Display(Name = "Recoverable")]
        public bool IsApproved { get; set; }
        public DateTime? DateApproved { get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        public string Reference { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Original Scope")]
        public bool IsOriginalScope { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [NotMapped]
        public string DisplayName => VariationNo == null ? null : VariationNo + ": " + Description;

        [JsonIgnore]
        public virtual Project Project { get; set; }
        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectVariationItem> ProjectVariationItems { get; set; }

        [JsonIgnore]
        public virtual ICollection<EmployeeTimesheetItem> EmployeeTimesheetItems { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return VariationID.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void SetLastModifiedUserAndTime(int userId)
        {
            LastModifiedBy = userId;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}