using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class AECOMUserClassification : IUserModified, ITrackableModel
    {
        [Key]
        [Display(Name = "ID")]
        public int AECOMUserClassificationId { get; set; }
        public int ProjectID { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Classification { get; set; }
        //[StringLength(511, ErrorMessage = "Maximum length is 511")]
        //public string Description { get; set; }

        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public string GetId()
        {
            return AECOMUserClassificationId.ToString();
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