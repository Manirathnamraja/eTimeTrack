using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectVariationItem : ITrackableModel, IUserModified, IMergeable
    {

        [Key]
        [Column(Order = 1)]
        public int VariationID { get; set; }
        [Key]
        [Column(Order = 2)]
        public int TaskID { get; set; }
        public bool IsApproved { get; set; }
        public bool IsClosed { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        public virtual ProjectVariation ProjectVariation { get; set; }
        [JsonIgnore]
        [ForeignKey("TaskID")]
        public virtual ProjectTask ProjectTask { get; set; }
        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return VariationID + " : " + TaskID;
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