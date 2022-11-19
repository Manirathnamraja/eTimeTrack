using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ReconciliationUpload : ITrackableModel, IUserModified
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public DateTime UploadDateTimeUtc { get; set; }
        public int ReconciliationTemplateId { get; set; }
        public int ProjectId { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }
        [JsonIgnore]
        public virtual ReconciliationTemplate ReconciliationTemplate { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> OriginalReconciliationEntries { get; set; }
        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> CurrentReconciliationEntries { get; set; }

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