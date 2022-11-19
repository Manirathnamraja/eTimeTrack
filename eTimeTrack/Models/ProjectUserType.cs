using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using eTimeTrack.Helpers;

namespace eTimeTrack.Models
{
    public class ProjectUserType : ITrackableModel, IUserModified
    {
        [Key]
        [Display(Name = "Project User Type ID")]
        public int ProjectUserTypeID { get; set; }
        public int ProjectID { get; set; }
        public int? UserTypeID { get; set; }
        public bool IsEnabled { get; set; }
        [DisplayName("Code Alias")]
        public string AliasCode { get; set; }
        [DisplayName("Type Alias")]
        public string AliasType { get; set; }
        [DisplayName("Description Alias")]
        public string AliasDescription { get; set; }
        //public float? MaxHoursOverall { get; set; }
        [DisplayName("Maximum NT Hours")]
        public float? MaxNTHours { get; set; }
        [DisplayName("Maximum OT1 Hours")]
        public float? MaxOT1Hours { get; set; }
        [DisplayName("Maximum OT2 Hours")]
        public float? MaxOT2Hours { get; set; }
        [DisplayName("Maximum OT3 Hours")]
        public float? MaxOT3Hours { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set;}
        //Added new Time Codes
        [DisplayName("Maximum OT4 Hours")]
        public float? MaxOT4Hours { get; set; }
        [DisplayName("Maximum OT5 Hours")]
        public float? MaxOT5Hours { get; set; }
        [DisplayName("Maximum OT6 Hours")]
        public float? MaxOT6Hours { get; set; }
        [DisplayName("Maximum OT7 Hours")]
        public float? MaxOT7Hours { get; set; }

        [JsonIgnore]
        [ForeignKey("UserTypeID")]
        public virtual UserType UserType { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [JsonIgnore]
        public virtual ICollection<EmployeeProject> EmployeeProjects { get; set; }

        //[JsonIgnore]
        //public virtual ICollection<ProjectUserTypeTimeCode> ProjectUserTypeTimeCodes { get; set; }

        public ProjectUserType()
        {
            IsEnabled = true;
            AliasCode = null;
            MaxNTHours = 40;
            MaxOT1Hours = null;
            MaxOT2Hours = null;
            MaxOT3Hours = null;
            //MaxHoursOverall = 40;
            //Added new Time Codes
            MaxOT4Hours = null;
            MaxOT5Hours = null;
            MaxOT6Hours = null;
            MaxOT7Hours = null;
            LastModifiedDate = DateTime.UtcNow;
            if (UserHelpers.GetCurrentUserId() != UserHelpers.Invalid)
            {
                LastModifiedBy = UserHelpers.GetCurrentUserId();
            }
        }

        public string GetId()
        {
            return ProjectUserTypeID.ToString();
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