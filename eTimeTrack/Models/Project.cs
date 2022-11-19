using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class Project : ITrackableModel, IUserModified, IMergeable
    {
        [Key]
        public int ProjectID { get; set; }
        [StringLength(15, ErrorMessage = "Maximum length is 15"), Required]
        [Display(Name = "Project Number")]
        public string ProjectNo { get; set; }
        public int? OfficeID { get; set; }
        [StringLength(5, ErrorMessage = "Maximum length is 5")]
        public string SeqNo { get; set; }
        [StringLength(5, ErrorMessage = "Maximum length is 5")]
        public string YearNo { get; set; }
        [StringLength(10, ErrorMessage = "Maximum length is 10")]
        public string RegistrationNo { get; set; }
        [StringLength(150, ErrorMessage = "Maximum length is 150")]
        public string Name { get; set; }
        public int? DirectorID { get; set; }
        [Display(Name = "Lead eTT Administrator")]
        public int? ManagerID { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
        public DateTime? DateClosed { get; set; }
        public DateTime? DateOpened { get; set; }
        public int? ClientCompanyID { get; set; }
        public int? ClientContactID { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Notes { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        [Display(Name = "Archived")]
        public bool IsArchived { get; set; }
        [Display(Name = "Display Task Notes to Users")]
        public bool DisplayTaskNotes { get; set; }
        [Display(Name = "Daily Comments Mandatory")]
        public bool CommentsMandatory { get; set; }
        public int? ProjectTimeCodeConfigId { get; set; }

        [JsonIgnore]
        public virtual Office Office { get; set; }
        [JsonIgnore]
        public virtual Employee Director { get; set; }
        [JsonIgnore]
        public virtual Employee Manager { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectGroup> ProjectGroups { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProjectPart> ProjectParts { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProjectVariation> ProjectVariations { get; set; }
        [JsonIgnore]
        public virtual ICollection<EmployeeProject> Employees { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProjectCompany> Companies { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectTimeCodeConfigId")]
        public virtual ProjectTimeCodeConfig ProjectTimeCodeConfig { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectUserType> ProjectUserTypes { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationUpload> ReconciliationUploads { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return ProjectID.ToString();
        }

        [NotMapped]
        public string DisplayName => ProjectNo + ": " + Name;

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