using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectGroup : ITrackableModel, IUserModified, IProjectComponent, IMergeable
    {
        [Key]
        public int GroupID { get; set; }
        [Required]
        [Display(Name="Project")]
        public int ProjectID { get; set; }
        [Display(Name = "Project Part")]
        public int PartID { get; set; }
        [StringLength(12, ErrorMessage = "Maximum length is 12"), Required]
        [Display(Name = "Group No")]
        public string GroupNo { get; set; }
        [StringLength(150, ErrorMessage = "Maximum length is 150")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Notes { get; set; }
        [Display(Name = "Group Type")]
        public int? GroupTypeID { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
        [StringLength(15, ErrorMessage = "Maximum length is 15")]
        [Display(Name = "Alias Code")]
        public string AliasCode { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? PM { get; set; }

        [NotMapped]
        public string DisplayName => GroupNo + ": " + Name;

        [JsonIgnore]
        public virtual Project Project { get; set; }
        [JsonIgnore]
        public virtual LU_GroupType GroupType { get; set; }

        [JsonIgnore]
        [ForeignKey("PartID")]
        public virtual ProjectPart ProjectPart { get; set; }

        [JsonIgnore]
        [ForeignKey("PM")]
        public virtual Employee PMUser { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectTask> Tasks { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return GroupID.ToString();
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
            return ProjectPart;
        }

        public ProjectGroup GetParentProjectGroup()
        {
            return null;
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