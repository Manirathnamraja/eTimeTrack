using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ReconciliationType : IUserModified, ITrackableModel
    {
        public int Id { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Text { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> ReconciliationEntries { get; set; }

        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public string GetId()
        {
            return Id.ToString();
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