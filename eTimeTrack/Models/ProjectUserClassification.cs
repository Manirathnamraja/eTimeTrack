using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectUserClassification : IUserModified, ITrackableModel
    {
        [Key]
        [Display(Name = "Project User Classifications")]
        public int ProjectUserClassificationId { get; set; }
        public int ProjectID { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        [Display(Name = "Project User Classification")]
        public string ProjectClassificationText { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Description { get; set; }
        [Display(Name = "AECOM User Classifications")]
        public int? AECOMUserClassificationID { get; set; }


        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        [ForeignKey("AECOMUserClassificationID")]
        public virtual AECOMUserClassification AECOMUserClassification { get; set; }

        public string GetId()
        {
            return ProjectUserClassificationId.ToString();
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