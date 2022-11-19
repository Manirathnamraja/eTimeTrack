using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ReconciliationTemplate : ITrackableModel, IUserModified
    {
        public int Id { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Name { get; set; }
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string EmployeeNumberColumn { get; set; }
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string WeekEndingColumn { get; set; }
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string HoursColumn { get; set; }
        [StringLength(2, ErrorMessage = "Maximum length is 2")]
        public string TypeIdentifierColumn { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string TypeIdentifierText { get; set; }
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }
        public bool DailyDates { get; set; }

        [JsonIgnore]
        public virtual Company Company { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationUpload> ReconciliationUploads { get; set; }

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