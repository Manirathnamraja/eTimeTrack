//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using eTimeTrack.Enums;
//using Newtonsoft.Json;

//namespace eTimeTrack.Models
//{
//    public class ProjectUserTypeTimeCode : ITrackableModel
//    {
//        [Key]
//        [Display(Name = "Project User Type Time Code ID")]
//        public int ProjectUserTypeTimeCodeID { get; set; }
//        public int ProjectUserTypeID { get; set; }
//        public TimeCode TimeCode { get; set; }
//        public float MaxHours { get; set; }

//        [JsonIgnore]
//        [ForeignKey("ProjectUserTypeID")]
//        public virtual ProjectUserType ProjectUserType { get; set; }

//        public string GetId()
//        {
//            throw new NotImplementedException();
//        }

//        public string ToJson()
//        {
//            return JsonConvert.SerializeObject(this);
//        }
//    }
//}