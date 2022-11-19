using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eTimeTrack.Models
{
    public class ProjectPart : ITrackableModel, IUserModified, IProjectComponent, IMergeable
    {
        [Key]
        public int PartID { get; set; }
        [Required]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        [StringLength(10, ErrorMessage = "Maximum length is 10"), Required]
        [Display(Name = "Part No")]
        public string PartNo { get; set; }
        [StringLength(150, ErrorMessage = "Maximum length is 150")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Notes { get; set; }
        [Display(Name="Closed")]
        public bool IsClosed { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? PM { get; set; }

        [NotMapped]
        public string DisplayName => PartNo + ": " + Name;

        [JsonIgnore]
        public virtual Project Project { get; set; }

        [JsonIgnore]
        [ForeignKey("PM")]
        public virtual Employee PMUser { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectGroup> ProjectGroups { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return PartID.ToString();
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
            return null;
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