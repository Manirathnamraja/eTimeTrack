using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectTask : ITrackableModel, IUserModified, IProjectComponent, IMergeable
    {
        [Key]
        public int TaskID { get; set; }
        [Display(Name="Project")]
        public int ProjectID { get; set; }
        [Required]
        [Display(Name = "Project Group")]
        public int GroupID { get; set; }
        [StringLength(30, ErrorMessage = "Maximum length is 30"), Required]
        [Display(Name = "Task No")]
        public string TaskNo { get; set; }
        [StringLength(150, ErrorMessage = "Maximum length is 150")]
        public string Name { get; set; }
        [StringLength(20, ErrorMessage = "Maximum length is 20")]
        [Display(Name = "Alias Code")]
        public string AliasCode { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        [Display(Name = "Oracle Code")]
        public string OracleCode { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Notes { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? PM { get; set; }

        [NotMapped]
        public string DisplayName => TaskNo + ": " + Name;

        [JsonIgnore]
        public virtual Project Project { get; set; }
        [ForeignKey("GroupID")]
        [JsonIgnore]
        public virtual ProjectGroup ProjectGroup { get; set; }

        [ForeignKey("PM")]
        [JsonIgnore]
        public virtual Employee PMUser { get; set; }

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
            return TaskID.ToString();
        }

        public string GetDisplayName()
        {
            return DisplayName;
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

        public Project GetParentProject()
        {
            return Project;
        }

        public ProjectPart GetParentProjectPart()
        {
            return ProjectGroup.ProjectPart;
        }

        public ProjectGroup GetParentProjectGroup()
        {
            return ProjectGroup;
        }

        public bool IsActive()
        {
            return !IsClosed;
        }

        public string GetName()
        {
            return Name;
        }

        public string GetDescription()
        {
            return Notes;
        }
    }
}