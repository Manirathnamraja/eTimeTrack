using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class LU_GroupType : ITrackableModel, IUserModified
    {
        [Key]
        public int GroupTypeID { get; set; }
        [StringLength(5, ErrorMessage = "Maximum length is 5")]
        public string GroupTypeCode { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProjectGroup> ProjectGroups { get; set; }
        public string GetId()
        {
            return GroupTypeID.ToString();
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