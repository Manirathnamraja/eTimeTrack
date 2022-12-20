using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectDiscipline : IUserModified, ITrackableModel
    {
        [Key]
        [Display(Name = "Project Discipline ID")]
        public int ProjectDisciplineId { get; set; }
        //public int ProjectID { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Text { get; set; }
        [StringLength(511, ErrorMessage = "Maximum length is 511")]
        public string Description { get; set; }

        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public string GetId()
        {
            return ProjectDisciplineId.ToString();
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